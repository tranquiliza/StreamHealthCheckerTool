using System;

namespace StreamHealthChecker.Core
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
