using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StreamHealthChecker.Core
{
    public interface IStreamHealthLogger
    {
        Task LogCurrentBitrate(int bitrate, string username);
    }
}
