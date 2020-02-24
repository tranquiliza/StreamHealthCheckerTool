using System;
using System.Collections.Generic;
using System.Text;

namespace StreamHealthChecker.Core
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
