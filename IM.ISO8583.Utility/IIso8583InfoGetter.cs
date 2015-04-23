using System.Xml.Linq;
using System.Collections.Generic;

namespace IM.ISO8583.Utility
{
    public interface IIso8583InfoGetter
    {
        //XElement GetInfo(string xPath);
        IList<BitIndex> GetInfos();
        void ResetInfos();
    }    
}

