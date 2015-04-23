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
using IM.ISO8583.Utility;
//
using Spring.Context;
using Spring.Context.Support;
//
namespace IM.ISO8583.Utility.UnitTest
{
    [TestFixture]
    public class TestIso8583InfoGetter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TestIso8583InfoGetter));   
        //
        private IApplicationContext ctx;
        private IIso8583InfoGetter postInfoGetter = null;

        [SetUp]
        public void InitContext()
        {
            this.ctx = ContextRegistry.GetContext();
            this.postInfoGetter = ctx["iso8583InfoGetter"] as IIso8583InfoGetter;
        }

        [Test]
        public void TestGetInfos()
        {
            IList<BitIndex> bitList = this.postInfoGetter.GetInfos();
            foreach ( BitIndex bi in bitList )
            {
                log.Debug(bi);
            }
        }

        [TearDown]
        public void TearDown()
        {  
        }
    }
}


