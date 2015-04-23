using System;
using System.Collections.Generic;

namespace IM.ISO8583.Utility
{
    public interface IBitWorker
    {
        int Length();
        //IList<BitIndex> GetBitList();
        BitIndex Get( int idx );
    }
}

