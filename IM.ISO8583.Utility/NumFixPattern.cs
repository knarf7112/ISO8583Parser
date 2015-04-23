using System;
using System.Collections.Generic;
using Common.Logging;
using Kms.Utility;

namespace IM.ISO8583.Utility
{
    /// <summary>
    /// The 'ConcreteFlyweight' class , for Numeric fields
    /// </summary>
    public class NumFixPattern : IPattern
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NumFixPattern));
        private int  length = 0;
        public NumFixPattern( int length )
        {
            this.length = length;
        }

        public void Build(MsgContext buildState)
        {
            try
            {
                IsoField currentField = buildState.CurrentField;
                string padData = currentField.FData.PadLeft(this.length, '0');
                currentField.FData = padData.Substring( padData.Length - this.length, this.length );
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
                string currentFData = parseState.SrcMessage.Substring( parseState.Start, this.length );
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
