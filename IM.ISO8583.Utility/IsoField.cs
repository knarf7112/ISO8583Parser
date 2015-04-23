using Kms.Utility;

namespace IM.ISO8583.Utility
{
    public class IsoField : AbstractDO
    {
        public virtual int FNo { get; set; }
        public virtual string FData { get; set; }
        public virtual int NextFNo { get; set; }
    }
}
