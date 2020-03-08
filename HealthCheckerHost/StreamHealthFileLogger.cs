using StreamHealthChecker.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheckerHost
{
    public class StreamHealthFileLogger : IStreamHealthLogger
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private const string _folderName = "";

        public StreamHealthFileLogger(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task LogCurrentBitrate(int bitrate, string username)
        {
            if (!string.IsNullOrEmpty(_folderName))
                Directory.CreateDirectory(_folderName);

            var currentLogFile = $"{_dateTimeProvider.UtcNow.ToString("yyyyMMdd")}_{username}.txt";

            var fileName = !string.IsNullOrEmpty(_folderName) ? Path.Combine(_folderName, currentLogFile) : currentLogFile;
            await File.AppendAllTextAsync(fileName, $"{_dateTimeProvider.UtcNow.ToString("yyyy'-'MM'-'dd HH':'mm':'ss")};{bitrate} kbps;" + Environment.NewLine).ConfigureAwait(false);
        }
    }
}
