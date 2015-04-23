using System;
//
using Kms.Utility;

namespace IM.ISO8583.Utility
{
    public class BitIndex : AbstractDO
    {
        public int Id { get; set; }
        public string Representation { get; set; }
        public string Name { get; set; }
        public IPattern PatternWorker { get; set; }
    }   
}
