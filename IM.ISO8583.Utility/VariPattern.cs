using System;
using System.Collections.Generic;
using Common.Logging;
using Kms.Utility;

namespace IM.ISO8583.Utility
{
    /// <summary>
    /// The 'ConcreteFlyweight' class , for none numeric fields
    /// </summary>
    public class VariPattern : IPattern
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(VariPattern));
        private int maxLength = 0;
        private int lenSize = 0;
        public VariPattern( int lenSize, int maxLength  )
        {
            this.lenSize = lenSize;
            this.maxLength = maxLength;
        }

        public void Build(MsgContext buildState)
        {
            try
            {
                // get raw data length
                IsoField currentField = buildState.CurrentField;
                int dataLen = currentField.FData.Length;
                string padLi = Convert.ToString( dataLen ).PadLeft(this.lenSize, '0');
                currentField.FData = padLi + currentField.FData;
                //                
                buildState.SrcMessage += currentField.FData;
                buildState.Start += currentField.FData.Length;
                buildState.BitMap += "1";
            }
            catch( Exception ex )
            {
                log.Error(ex.StackTrace);
                throw ex;
            }
        }

        public void Parse(MsgContext parseState)
        {
            try
            {                
                //use isoField.NextFNo as current field number 
                int currentFno = parseState.CurrentField.NextFNo;
                int dataLen = Convert.ToInt32( parseState.SrcMessage.Substring( parseState.Start, this.lenSize ) );
                parseState.Start += this.lenSize;
                string currentFData = parseState.SrcMessage.Substring( parseState.Start, dataLen );
                parseState.Start += dataLen;
                parseState.AddField(currentFno, currentFData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
