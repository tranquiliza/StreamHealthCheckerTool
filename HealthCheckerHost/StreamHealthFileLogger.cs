using StreamHealthChecker.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HealthCheckerHost
{
    public class StreamHealthFileLogger : IStreamHealthLogger
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly string _folderName;

        public StreamHealthFileLogger(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _folderName = AppDomain.CurrentDomain.BaseDirectory;
        }

        public async Task LogCurrentBitrate(int bitrate, string username)
        {
            if (!string.IsNullOrEmpty(_folderName))
                Directory.CreateDirectory(_folderName);

            var currentLogFile = $"{_dateTimeProvider.UtcNow.ToString("yyyyMMdd")}_{username}.csv";

            var logFileName = !string.IsNullOrEmpty(_folderName) ? Path.Combine(_folderName, currentLogFile) : currentLogFile;
            await File.AppendAllTextAsync(logFileName, $"{_dateTimeProvider.UtcNow.ToString("yyyy'-'MM'-'dd HH':'mm':'ss")};{bitrate} kbps;" + Environment.NewLine).ConfigureAwait(false);
            var userFileName = !string.IsNullOrEmpty(_folderName) ? Path.Combine(_folderName, username) : username;
            await File.WriteAllTextAsync($"{userFileName}.txt", $"{bitrate} kbps").ConfigureAwait(false);
        }
    }
}
