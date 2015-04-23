using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Common.Logging;

namespace IM.ISO8583.Utility
{
    public class MainMsgWorker : IMsgWorker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainMsgWorker));
        //
        public IBitMapHelper BitMapHelper { private get; set; }
        public IBitWorker BitWorker { private get; set; }

        public MainMsgWorker() { }
        public MsgContext Parse(string msg)
        {
            try
            {
                MsgContext msgContextMain = new MsgContext();
                msgContextMain.SrcMessage = msg;
                //
                msgContextMain.MsgSize = Convert.ToInt32( msgContextMain.SrcMessage.Substring(msgContextMain.Start, 3), 10 );
                if (msgContextMain.SrcMessage.Length != msgContextMain.MsgSize + 3)
                {
                    //log.Error("Message size error, expect " + msgContextMain.MsgSize + " but " + (msgContextMain.SrcMessage.Length - 3));
                    throw new Exception( "Message size error, expect:[" + msgContextMain.MsgSize + "] but:[" + (msgContextMain.SrcMessage.Length - 3) + "]" );
                }
                msgContextMain.Start =+ 3;
                msgContextMain.FromTo = msgContextMain.SrcMessage.Substring(msgContextMain.Start, 8);
                msgContextMain.Start = msgContextMain.Start + 8;
                //
                msgContextMain.Mti = msgContextMain.SrcMessage.Substring(msgContextMain.Start, 4);
                msgContextMain.Start = msgContextMain.Start + 4;
                //
                bool hasSecondMap = this.BitMapHelper.HasExtend(msgContextMain.SrcMessage.Substring(msgContextMain.Start, 2));
                int bitLen = hasSecondMap ? 32 : 16;
                string bitMapHex = msgContextMain.SrcMessage.Substring(msgContextMain.Start, bitLen);
                msgContextMain.Start = msgContextMain.Start + bitLen;
                msgContextMain.BitMap = this.BitMapHelper.GetBitMapBits(bitMapHex);
                //
                for (int fno = 2; fno <= msgContextMain.BitMap.Length; fno++)
                {
                    if (msgContextMain.HasField(fno))
                    {
                        BitIndex bi = this.BitWorker.Get(fno);
                        IPattern pattern = bi.PatternWorker;
                        msgContextMain.CurrentField.NextFNo = fno;
                        pattern.Parse(msgContextMain);
                    }
                }
                return msgContextMain;
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
                MsgContext msgContextMain = new MsgContext();
                msgContextMain.FromTo = fromTo;
                msgContextMain.Mti = mti;
                // BitMap, always has SecondMap in Common
                if (65 == fDatas.Length)
                {
                    msgContextMain.BitMap = "0";
                }
                else
                {
                    msgContextMain.BitMap = "1";
                }
                //
                //string[] srcList = new string[this.BitWorker.Length()];
                string[] srcList = new string[fDatas.Length];
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
                    if (!( (null == srcList[fno]) || ("".Equals(srcList[fno]))))
                    {
                        BitIndex bi = this.BitWorker.Get(fno);
                        IPattern pattern = bi.PatternWorker;
                        msgContextMain.AddField(fno, srcList[fno]);
                        pattern.Build(msgContextMain);
                    }
                    else
                    {
                        msgContextMain.BitMap += "0";
                    }
                }
                msgContextMain.SrcMessage = msgContextMain.FromTo + msgContextMain.Mti + this.BitMapHelper.GetBitMapHex(msgContextMain.BitMap) + msgContextMain.SrcMessage;
                msgContextMain.MsgSize = msgContextMain.SrcMessage.Length;
                msgContextMain.SrcMessage = msgContextMain.MsgSize.ToString("D3") + msgContextMain.SrcMessage;
                log.Debug("MsgSource:{" + msgContextMain.SrcMessage + "}");
                return msgContextMain;
            } catch( Exception ex)
            {
                log.Error(ex.Message);
            }
            return null;
        }
    }
}
