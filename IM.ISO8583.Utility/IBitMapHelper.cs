using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IM.ISO8583.Utility
{
    public interface IBitMapHelper
    {
        bool HasExtend(string hexStr);
        string GetBitMapBits(string hexStr);
        string GetBitMapHex(string bitStr);
    }
}
