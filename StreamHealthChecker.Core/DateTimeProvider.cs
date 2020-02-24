using System;
using System.Collections.Generic;
using System.Text;

namespace StreamHealthChecker.Core
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
