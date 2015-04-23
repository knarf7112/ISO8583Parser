using System;
using System.Collections.Generic;
using Common.Logging;
using Kms.Utility;

namespace IM.ISO8583.Utility
{
    /// <summary>
    /// The 'ConcreteFlyweight' class , for none numeric fields
    /// </summary>
    public class StrFixPattern : IPattern
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StrFixPattern));
        private int  length = 0;
        public StrFixPattern( int length )
        {
            this.length = length;
        }

        public void Build(MsgContext buildState)
        {
            try
            {
                IsoField currentField = buildState.CurrentField;
                string padData = currentField.FData.PadRight(this.length, ' ');
                currentField.FData = padData.Substring(0, this.length);
                buildState.SrcMessage += currentField.FData;
                buildState.Start += this.length;
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
                string currentFData = parseState.SrcMessage.Substring(parseState.Start, this.length);
                parseState.Start += this.length;
                parseState.AddField(currentFno, currentFData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
