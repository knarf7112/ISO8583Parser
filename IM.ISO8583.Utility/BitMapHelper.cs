using System;
using System.Collections.Generic;
using Common.Logging;
using Kms.Utility;
using Kms.Crypto;

namespace IM.ISO8583.Utility
{
    /// <summary>
    /// Check and convert bitmap
    /// </summary>
    public class BitMapHelper : IBitMapHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BitMapHelper));
        public IBitMapper BitMapper { private get; set; }
        public IHexConverter HexConverter { private get; set; }
        public BitMapHelper()
        {
        }

        public bool HasExtend( string hexStr )
        {            
            byte first = this.HexConverter.Hex2Byte( hexStr.Substring(0, 2) );
            return (0x80 == (first & 0x80));
        }

        public string GetBitMapBits( string hexStr )
        {
            return this.BitMapper.Hex2Bits(hexStr);
        }

        public string GetBitMapHex( string bitStr )
        {
            return this.BitMapper.Bits2Hex(bitStr);
        }
    }
}
