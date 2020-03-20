using Flurl.Http;
using StreamHealthChecker.Core.Model;
using StreamHealthChecker.TwitchIntegration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamHealthChecker.Core.Application
{
    //responses: 
    //    "WARNING: No OBS connection"
    //    "Stream not live!"
    //    "Video type: H264 High 3.1 30fps | Current Bitrate: 2431 kbps"
    //    "Video type: No connection | Current Bitrate: 0 kbps"
    //    "Video type: Unmanaged scene | Current Bitrate: NaN kbps"

    // If stream is not live, retry every 10 minutes?
    // If stream is live, check every 10 seconds
    // If stream goes from not live -> Live -> Send message in chat to let us know we working (Send the health check values)
    // If no connection, let us know.
    // If bitrate below configured -> Let us know.
    // No OBS Connection Check every 10 Minutes

    public class UserJob : IDisposable
    {
        private enum StreamState
        {
            NoObs = 1,
            NotLive = 2,
            NotIrlStream = 3,
            Live = 4
        }

        private const string _noConnectionValue = "No connection";
        private const string _noObsValue = "WARNING: No OBS connection";
        private const string _streamNotLiveValue = "Stream not live!";
        private const string _streamNotIrl = "Video type: Unmanaged scene | Current Bitrate: NaN kbps";
        private const string _noResponse = "API Not responding";

        private bool _hasJustSentError = false;
        private string _currentHealthInfomation;
        private bool _poorConnection = false;

        private readonly User _user;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Random _random = new Random();
        private StreamState _lastState;
        private readonly ITwitchIntegration _twitchIntegration;
        private readonly IStreamHealthLogger _streamHealthLogger;

        public UserJob(User user, ITwitchIntegration twitchIntegration, IStreamHealthLogger streamHealthLogger)
        {
            _user = user;
            _twitchIntegration = twitchIntegration;
            _streamHealthLogger = streamHealthLogger;
        }

        public Task Run()
        {
            return Task.Run(async () =>
            {
                // Wait for 10 seconds, so we can actually send a message on startup!
                await WaitFor(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                while (!_cts.IsCancellationRequested)
                {
                    switch (await CheckState().ConfigureAwait(false))
                    {
                        case StreamState.NoObs:
                            await WaitFor(_user.NoObsCheckInterval).ConfigureAwait(false);
                            break;
                        case StreamState.NotLive:
                        case StreamState.NotIrlStream:
                            await WaitFor(_user.NotLiveCheckInternval).ConfigureAwait(false);
                            break;
                        case StreamState.Live:
                            await RunHealthCheck().ConfigureAwait(false);
                            if (_hasJustSentError)
                            {
                                await WaitFor(_user.LiveCheckJustSentErrorInterval).ConfigureAwait(false);
                                _hasJustSentError = false;
                            }
                            else
                            {
                                await WaitFor(_user.LiveCheckInterval).ConfigureAwait(false);
                            }

                            break;
                    }
                }
            }, _cts.Token);
        }

        private async Task WaitFor(TimeSpan timeSpan)
        {
            await Task.Delay(timeSpan).ConfigureAwait(false);
        }

        private async Task RunHealthCheck()
        {
            if (_currentHealthInfomation.Contains(_noConnectionValue))
            {
                await SendNoConnectionMessage().ConfigureAwait(false);

                _poorConnection = true;
                _hasJustSentError = true;
                return;
            }

            var currentBitrate = ExtractBitrate();
            await _streamHealthLogger.LogCurrentBitrate(currentBitrate, _user.Username).ConfigureAwait(false);

            if (currentBitrate < _user.MinimumBitrate)
            {
                if (!_hasJustSentError)
                    await SendPoorConnectionMessage(currentBitrate).ConfigureAwait(false);

                _poorConnection = true;
                _hasJustSentError = true;
                return;
            }

            if (_poorConnection)
            {
                _poorConnection = false;
                await SendMessageToUser($"ISSUES RESOLVED! {currentBitrate} kbps").ConfigureAwait(false);
            }
        }

        // This is currently very crude, if the string is not exactly what we expect, we always get a 0. 
        // We could instead, iterate through all the values, and take the one that successfully parses.
        // Bitrate will never be a decimal, so if we meet a decimal, that is parseable, we need to discard that value and continue.
        private int ExtractBitrate()
        {
            var stringSplit = _currentHealthInfomation.Split(' ');
            if (stringSplit.Length != 11)
                return 0;

            if (int.TryParse(stringSplit[9], out var bitrate))
                return bitrate;

            return 0;
        }

        private async Task SendNoConnectionMessage()
        {
            var messages = _user.NoConnectionMessages;
            if (messages.Count == 0)
                await SendMessageToUser("ISSUES DETECTED: No Connection!").ConfigureAwait(false);
            else
                await SendMessageToUser(messages[_random.Next(0, messages.Count)]).ConfigureAwait(false);
        }

        private async Task SendPoorConnectionMessage(int currentBitrate)
        {
            var messages = _user.PoorConnectionMessages;
            if (messages.Count == 0)
                await SendMessageToUser($"ISSUES DETECTED: {currentBitrate} kbps").ConfigureAwait(false);
            else
                await SendMessageToUser(messages[_random.Next(0, messages.Count)] + $" {currentBitrate} kbps").ConfigureAwait(false);
        }

        private async Task SendMessageToUser(string message)
        {
            await _twitchIntegration.SendMessage(_user.Username, message).ConfigureAwait(false);
        }

        private async Task<StreamState> CheckState()
        {
            _currentHealthInfomation = await GetStreamInfo().ConfigureAwait(false);

            if (string.Equals(_noObsValue, _currentHealthInfomation, StringComparison.OrdinalIgnoreCase))
            {
                _lastState = StreamState.NoObs;
                return StreamState.NoObs;
            }

            if (string.Equals(_streamNotLiveValue, _currentHealthInfomation, StringComparison.OrdinalIgnoreCase))
            {
                _lastState = StreamState.NotLive;
                return StreamState.NotLive;
            }

            if (string.Equals(_streamNotIrl, _currentHealthInfomation))
            {
                return StreamState.NotIrlStream;
            }

            if (string.Equals(_noResponse, _currentHealthInfomation))
            {
                _lastState = StreamState.NotLive;
                return StreamState.NotLive;
            }

            if (_lastState != StreamState.Live)
            {
                await SendMessageToUser($"Hello {_user.Username} I am now monitoring your stream, have a great stream!").ConfigureAwait(false);
                _lastState = StreamState.Live;
            }

            return StreamState.Live;
        }

        private async Task<string> GetStreamInfo()
        {
            try
            {
                return await _user.HealthApiUrl.GetStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Todo Log Error EX
                return _noResponse;
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
