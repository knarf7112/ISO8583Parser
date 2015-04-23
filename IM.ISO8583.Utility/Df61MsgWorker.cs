using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Common.Logging;

namespace IM.ISO8583.Utility
{
    public class Df61MsgWorker : IMsgWorker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Df61MsgWorker));
        //
        public IBitMapHelper BitMapHelper { private get; set; }
        public IBitWorker Df61BitWorker { private get; set; }

        public Df61MsgWorker() { }
        public MsgContext Parse(string msg)
        {
            try
            {
                MsgContext msgContext = new MsgContext();
                // only 64 fields,No Second map
                msgContext.BitMap = "0";
                msgContext.SrcMessage = msg;  
                //
                int bitLen = 16;
                string bitMapHex = msgContext.SrcMessage.Substring(msgContext.Start, bitLen);
                msgContext.Start = msgContext.Start + bitLen;
                msgContext.BitMap = this.BitMapHelper.GetBitMapBits(bitMapHex);
                //
                for (int fno = 2; fno <= msgContext.BitMap.Length; fno++)
                {
                    if (msgContext.HasField(fno))
                    {
                        BitIndex bi = this.Df61BitWorker.Get(fno);
                        IPattern pattern = bi.PatternWorker;
                        msgContext.CurrentField.NextFNo = fno;
                        pattern.Parse(msgContext);
                    }
                }
                return msgContext;
            }
            catch( Exception ex )
            {
                log.Error(ex.Message);
            }
            return null;
        }

        public MsgContext Build( string fromTo, string mti, string[] fDatas )
        {
            try
            {
                MsgContext msgContext = new MsgContext();
                // only 64 fields
                msgContext.BitMap = "0";                
                string[] srcList = new string[this.Df61BitWorker.Length()];
                for (int i = 0; i < srcList.Length; i++)
                {
                    if (i < fDatas.Length)
                    {
                        srcList[i] = fDatas[i];
                    }
                    else
                    {
                        srcList[i] = "";
                    }
                }               
                //
                for (int fno = 2; fno < srcList.Length; fno++)
                {
                    if (!((null == srcList[fno]) || ("".Equals(srcList[fno]))))
                    {
                        BitIndex bi = this.Df61BitWorker.Get(fno);
                        IPattern pattern = bi.PatternWorker;
                        msgContext.AddField(fno, srcList[fno]);
                        pattern.Build(msgContext);
                    }
                    else
                    {
                        msgContext.BitMap += "0";
                    }
                }
                msgContext.SrcMessage = this.BitMapHelper.GetBitMapHex(msgContext.BitMap) + msgContext.SrcMessage;
                return msgContext;
            } catch( Exception ex)
            {
                log.Error(ex.Message);
            }
            return null;
        }
    }
}
