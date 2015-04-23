using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IM.ISO8583.Utility
{
    public interface IMsgWorker
    {
        MsgContext Parse( string msg );
        MsgContext Build( string fromTo, string mti, string[] srcList );
    }
}
