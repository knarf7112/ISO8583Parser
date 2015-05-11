using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
//
using Common.Logging;
//
using NUnit.Framework;
//
using Kms.Utility;
using Kms.Crypto;
using IM.ISO8583.Utility;
//
using Spring.Context;
using Spring.Context.Support;
//
namespace IM.ISO8583.Utility.UnitTest
{
    [TestFixture]
    public class TestMsgWorker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TestMsgWorker));
        //
        private IApplicationContext ctx;
        private IMsgWorker mainMsgWorker = null;
        private IMsgWorker df61MsgWorker = null;

        [SetUp]
        public void InitContext()
        {
            this.ctx = ContextRegistry.GetContext();
            this.mainMsgWorker = ctx["mainMsgWorker"] as IMsgWorker;
            this.df61MsgWorker = ctx["df61MsgWorker"] as IMsgWorker;
        }

        [Test]
        public void Test01Build0100Df61()
        {
            string[] srcList = new string[65]; // primary + 1
            for (int i = 0; i < srcList.Length; i++)
            {
                srcList[i] = "";
            }
            /*
            03 : 00000001
            04 : 001
            08 : 20150128183005
            10 : 7111680123456789
            11 : 55
            35 : 86000000000000000001
            */
            srcList[3] = "00000001";
            srcList[4] = "001";
            srcList[8] = "20150128183005";
            srcList[10] = "7111680123456789";
            srcList[11] = "55";
            srcList[35] = "86000000000000000001";
            //
            MsgContext df61MsgContext = this.df61MsgWorker.Build(null, null, srcList);

            //
            IList<IsoField> fList = df61MsgContext.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(df61MsgContext.SrcMessage);
        }

        [Test]
        public void Test02Parse0100Df61()
        {
            string msg = @"3160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            //
            MsgContext msgContext = this.df61MsgWorker.Parse(msg);

            IList<IsoField> fList = msgContext.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
        }

        [Test]
        public void Test03Build0100()
        {
            string expected = //"2318888082201007220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
                              //  "215"
            "8888082201007220000108C0000816000000000000000099017400000000005501281830055555550800000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            string fromTo = "88880822";
            string mti = "0100";

            string[] srcList = new string[65]; // primary + 1
            //for (int i = 0; i < srcList.Length; i++)
            //{
            //    srcList[i] = "";
            //}
            /* 
02 : 160000000000000000
03 : 990174
04 : 000000000055
07 : 0128183005
11 : 555555
32 : 10st00000001
37 : 502818555555
41 : 00000001
42 : 000000022555003
61 : 0853160000020000000000000010012015012818300571116801234567890000005586000000000000000001*/
            srcList[2] = "0000000000000000";
            srcList[3] = "990174";
            srcList[4] = "000000000055";
            srcList[7] = "0128183005";
            srcList[11] = "555555";
            srcList[32] = "00000001";
            srcList[37] = "502818555555";
            srcList[41] = "00000001";
            srcList[42] = "000000022555003";
            // need construct latter
            srcList[61] = this.getBuild0100Df61();
            //
            MsgContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);
            //
            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.SrcMessage);
            Assert.AreEqual(expected, msgContextMain.SrcMessage);
        }

        private string getBuild0100Df61()
        {
            string[] srcList = new string[65];
            for (int i = 0; i < srcList.Length; i++)
            {
                srcList[i] = "";
            }
            /*
            03 : 00000001
            04 : 001
            08 : 20150128183005
            10 : 7111680123456789
            11 : 55
            35 : 86000000000000000001
            */
            srcList[3] = "00000001";
            srcList[4] = "001";
            srcList[8] = "20150128183005";
            srcList[10] = "7111680123456789";
            srcList[11] = "55";
            srcList[35] = "86000000000000000001";
            //
            MsgContext msgContext = this.df61MsgWorker.Build(null, null, srcList);
            return msgContext.SrcMessage;
        }

        [Test]
        public void Test04Parse0100()
        {
            string msg = //@"231888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            //                "215"
            "8888082201007220000108C0000816000000000000000099017400000000005501281830055555550800000001502818555555000000010000000225550030853160000020000000000000010012015012818300571116801234567890000005586000000000000000001";
            MsgContext msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            msg = msgContextMain.GetField(61).FData;
            MsgContext msgContextDf61 = this.df61MsgWorker.Parse(msg);
            fList = msgContextDf61.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
        }

        [Test]
        public void Test05Parse0110()
        {
            string message =
                //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300099999910st00000001502818999999000000010000000225550030853160000020000000000000010012015012818300000000000000000000000005511111111111111111111";
                //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300500000000000000000000005586000000000000000001";
                //@"145082288880110F22000010AC000000000000000000000160000000000000000990174000000000055012818300555555510st000000015028185555550000000001000000022555003";
            //  "129"
            "082288880110722000010AC00000160000000000000000990174000000000055012818300555555508000000015028185555550000000001000000022555003";
            MsgContext msgContext = this.mainMsgWorker.Parse(message);
            //
            IList<IsoField> fList = msgContext.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
        }

        [Test]
        public void Test06Build0110()
        {
            string expected =
                //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300099999910st00000001502818999999000000010000000225550030853160000020000000000000010012015012818300000000000000000000000005511111111111111111111";
                //@"888808220100F220000108C000080000000000000000160000000000000000990174000000000055012818300555555510st00000001502818555555000000010000000225550030853160000020000000000000010012015012818300500000000000000000000005586000000000000000001";
                //@"145082288880110F22000010AC000000000000000000000160000000000000000990174000000000055012818300555555510st000000015028185555550000000001000000022555003";
            //    @"129"
            "082288880110722000010AC00000160000000000000000990174000000000055012818300555555508000000015028185555550000000001000000022555003";
            string fromTo = "08228888";
            string mti = "0110";
            string[] srcList = new string[65];
            /*
02 : 160000000000000000
03 : 990174
04 : 000000000055
07 : 0128183005
11 : 555555
32 : 10st00000001
37 : 502818555555
39 : 00
41 : 00000001
42 : 000000022555003
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990174";
            srcList[4] = "000000000055";
            srcList[7] = "0128183005";
            srcList[11] = "555555";
            srcList[32] = "00000001";
            srcList[37] = "502818555555";
            srcList[39] = "00";
            srcList[41] = "00000001";
            srcList[42] = "000000022555003";
            //
            MsgContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);
            //
            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.SrcMessage);
            Assert.AreEqual(expected, msgContextMain.SrcMessage);
        }

        [Test]
        public void Test07Parse0302()
        {
            string msg = //@"101"
            "082288880302E2240000080000000000002000000000160000000000000000990176012818000777777715125028187777771";
            MsgContext msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.FromTo);
            log.Debug(msgContextMain.Mti);
        }

        [Test]
        public void Test08Build0302()
        {
            MsgContext msgContextMain = null;
            string expected = //@"101"
            "082288880302E2240000080000000000002000000000160000000000000000990176012818000777777715125028187777771";
            string fromTo = "08228888";
            string mti = "0302";
            //
            string[] srcList = new string[129];
            for (int i = 0; i < srcList.Length; i++)
            {
                srcList[i] = "";
            }
            /* 
02 : 160000000000000000
03 : 990176
07 : 0128180007
11 : 777777
14 : 1512
37 : 502818777777
91 : 1
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990176";
            srcList[7] = "0128180007";
            srcList[11] = "777777";
            srcList[14] = "1512";
            srcList[37] = "502818777777";
            srcList[91] = "1";
            //
            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Test11Buid0420()
        {
            string expected = //@"212"
            "888808220420F220000108C000080000004000000000160000000000000000990174000000000055012818000666666610st00000001502818666666000000010000000225550030240080000000000000000000000100012818300555555500000001502818666666  ";
            string fromTo = "88880822";
            string mti = "0420";
            //
            string[] srcList = new string[129];
            //for (int i = 0; i < srcList.Length; i++)
            //{
            //    srcList[i] = "";
            //}
            /* 
02 : 0000000000000000
03 : 990174
04 : 000000000055
07 : 0128183006
11 : 666666
32 : st00000001
37 : 502818666666
41 : 00000001
42 : 000000022555003
61 : 008000000000000000000000
90 : 0100 0128183005 555555 10st00000001  
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990174";
            srcList[4] = "000000000055";
            srcList[7] = "0128180006";
            srcList[11] = "666666";
            srcList[32] = "st00000001";
            srcList[37] = "502818666666";
            srcList[41] = "00000001";
            srcList[42] = "000000022555003";
            //
            string[] srcListDf61 = new string[65];
            //for (int i = 0; i < srcListDf61.Length; i++)
            //{
            //    srcListDf61[i] = "";
            //}
            srcListDf61[9] = "00000000";
            MsgContext msgContextDf61 = this.df61MsgWorker.Build(null, null, srcListDf61);
            //
            srcList[61] = msgContextDf61.SrcMessage; //"008000000000000000000000";
            Assert.AreEqual("008000000000000000000000", srcList[61]);
            //
            srcList[90] = "0100" + "0128183005" + "555555" + "00000001" + "502818666666" + "  ";
            //
            MsgContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug("[" + result + "]");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Test12Parse0420()
        {
            string msg = //@"212"
            "888808220420F220000108C000080000004000000000160000000000000000990174000000000055012818000666666610st00000001502818666666000000010000000225550030240080000000000000000000000100012818300555555500000001502818666666  ";

            MsgContext msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }

            log.Debug(msgContextMain.FromTo + ":" + msgContextMain.Mti);
        }

        [Test]
        public void Test13Buid0430()
        {
            string expected = //@"187082288880420F220000108C000080000004000000000160000000000000000990174000000000055012818000666666610st00000001502818666666000000010000000225550030240080000000000000000000000100012818300555555500000001502818666666  ";
                              //  @"187"
            "082288880430F22000010AC000000000004000000000160000000000000000990174000000000055012818000666666610st0000000150281866666600000000010000000225550030100012818300555555500000001502818666666  ";
            string fromTo = "08228888";
            string mti = "0430";
            //
            string[] srcList = new string[129];
            /* 
02 : 160000000000000000
03 : 990174
04 : 000000000055
07 : 0128183006
11 : 666666
32 : 10st00000001
37 : 502818666666
39 : 00
41 : 00000001
42 : 000000022555003
90 : 0100012818300555555500000001502818666666  "   
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990174";
            srcList[4] = "000000000055";
            srcList[7] = "0128180006";
            srcList[11] = "666666";
            srcList[32] = "st00000001";
            srcList[37] = "502818666666";
            srcList[39] = "00";
            srcList[41] = "00000001";
            srcList[42] = "000000022555003";
            srcList[90] = "0100" + "0128183005" + "555555" + "00000001" + "502818666666" + "  ";
            //
            MsgContext msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug("[" + result + "]");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Test14Parse0430()
        {
            string msg = //@"187082288880430F22000010AC000000000004000000000160000000000000000990174000000000055012818000666666610st0000000150281866666600000000010000000225550030100012818300555555500000001502818666666  ";
                         //@"119"
            "888808220312622400000A0000081600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007"; 
            MsgContext msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }

            log.Debug(msgContextMain.FromTo + ":" + msgContextMain.Mti);
        }

        [Test]
        public void Test09Build0312()
        {
            MsgContext msgContextMain = null;
            string expected = //@"135888808220312E22400000A00000800000000000000001600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007";
                              //  @"119"
            "888808220312622400000A0000081600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007";
            string fromTo = "88880822";
            string mti = "0312";

            string[] srcList = new string[65];
            /* 
02 : 0000000000000000
03 : 990176
07 : 0128180007
11 : 777777
14 : 1512
37 : 502818777777
39 : 00
61 : 030010000000000000020150128180007            
             */
            srcList[2] = "0000000000000000";
            srcList[3] = "990176";
            srcList[7] = "0128180007";
            srcList[11] = "777777";
            srcList[14] = "1512";
            srcList[37] = "502818777777";
            srcList[39] = "00";
            srcList[61] = getBuild0312Df61();
            //
            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug(result);
            Assert.AreEqual(expected, result);
        }

        private string getBuild0312Df61()
        {
            string[] srcList = new string[65]; // primary + 1
            for (int i = 0; i < srcList.Length; i++)
            {
                srcList[i] = "";
            }
            /*
            08: 20150128180007
            */
            srcList[8] = "20150128180007";
            //
            MsgContext df61MsgContext = this.df61MsgWorker.Build(null, null, srcList);

            return df61MsgContext.SrcMessage;
        }

        [Test]
        public void Test10Parse0312()
        {
            MsgContext msgContextMain = null;
            string msg = //@"135"
            "888808220312E22400000A00000800000000000000001600000000000000009901760128180007777777151250281877777700030010000000000000020150128180007";
            msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.FromTo);
            log.Debug(msgContextMain.Mti);
            // parse df61
            IsoField fd = null;
            MsgContext msgContextDf61 = null;

            if (null != (fd = msgContextMain.GetField(61)))
            {
                msg = fd.FData;
                msgContextDf61 = this.df61MsgWorker.Parse(msg);
                fList = msgContextDf61.FieldList.ToList();
                foreach (IsoField field in fList)
                {
                    log.Debug(field);
                }
            }
        }

        [Test]
        public void Test21ParseSignOnRequest()
        {
            MsgContext msgContextMain = null;
            string msg = //"063"
            "082288880800822000000000000004000000000000000128132501111111071";
            msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.FromTo);
            log.Debug(msgContextMain.Mti);
        }

        [Test]
        public void Test22BuildSignOnResponse()
        {
            MsgContext msgContextMain = null;
            string expected = //@"082288880800822000000000000004000000000000000128132501111111071";
            //    "063"
            "888808220810822000000000000004000000000000000128132501111111071";
            string fromTo = "88880822";
            string mti = "0810";

            string[] srcList = new string[129];
            //07 : 0128132501
            //11 : 111111
            //70 : 071
            srcList[7] = "0128132501";
            srcList[11] = "111111";
            srcList[70] = "071"; // sign on response
            //
            msgContextMain = this.mainMsgWorker.Build( fromTo, mti, srcList );

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Test23ParseSignOffRequest()
        {
            MsgContext msgContextMain = null;
            string msg = //"063"
            "082288880800822000000000000004000000000000000128132502222222072";
            msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.FromTo);
            log.Debug(msgContextMain.Mti);
        }

        [Test]
        public void Test24BuildSignOffResponse()
        {
            MsgContext msgContextMain = null;
            string expected = //@"063"
            "888808220810822000000000000004000000000000000128132502222222072";
            string fromTo = "88880822";
            string mti = "0810";
            string[] srcList = new string[129];
            //07 : 0128132502
            //11 : 222222
            //70 : 072
            srcList[7] = "0128132502";
            srcList[11] = "222222";
            srcList[70] = "072"; // sign off response
            //
            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Test17ParseEcho1Request()
        {
            MsgContext msgContextMain = null;
            string msg = //"063"
            "082288880800822000000000000004000000000000000128132503333333301";
            msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.FromTo);
            log.Debug(msgContextMain.Mti);
        }

        [Test]
        public void Test18BuildEcho1Response()
        {
            MsgContext msgContextMain = null;
            string expected = //@"065"
            "88880822081082200000020000000400000000000000012813250333333300301";
            string fromTo = "88880822";
            string mti = "0810";
            string[] srcList = new string[129];
            //07 : 0128132503
            //11 : 333333
            //39 : 00
            //70 : 301
            srcList[7] = "0128132503";
            srcList[11] = "333333";
            srcList[39] = "00";
            srcList[70] = "301"; // sign off response
            //
            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Test20ParseEcho2Response()
        {
            MsgContext msgContextMain = null;
            string msg = //"065"
            "08228888081082200000020000000400000000000000012813250333333300301";
            msgContextMain = this.mainMsgWorker.Parse(msg);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            log.Debug(msgContextMain.FromTo);
            log.Debug(msgContextMain.Mti);
        }

        [Test]
        public void Test19BuildEcho2Request()
        {
            MsgContext msgContextMain = null;
            string expected = //@"063"
            "888808220800822000000000000004000000000000000128132503333333301";
            string fromTo = "88880822";
            string mti = "0800";
            string[] srcList = new string[129];
            srcList[7] = "0128132503";
            srcList[11] = "333333";
            //srcList[39] = "00";
            srcList[70] = "301"; // echo 2 request
            //
            msgContextMain = this.mainMsgWorker.Build(fromTo, mti, srcList);

            IList<IsoField> fList = msgContextMain.FieldList.ToList();
            foreach (IsoField field in fList)
            {
                log.Debug(field);
            }
            string result = msgContextMain.SrcMessage;
            log.Debug(result);
            Assert.AreEqual(expected, result);
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}


