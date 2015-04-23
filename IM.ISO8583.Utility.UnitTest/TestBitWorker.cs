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
    public class TestBitWorker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TestBitWorker));   
        //
        private IApplicationContext ctx;
        private IBitWorker bitWorker = null;

        [SetUp]
        public void InitContext()
        {
            this.ctx = ContextRegistry.GetContext();
            this.bitWorker = ctx["bitWorker"] as IBitWorker;
        }

        [Test]
        public void TestGetInfos()
        {
            for (int i = 0; i < this.bitWorker.Length(); i++  )
            {
                BitIndex bi = this.bitWorker.Get(i);
                log.Debug(bi);
            }
        }

        [TearDown]
        public void TearDown()
        {  
        }
    }
}


