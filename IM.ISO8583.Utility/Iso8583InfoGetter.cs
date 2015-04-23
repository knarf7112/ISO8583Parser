using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
//
using Common.Logging;
//
namespace IM.ISO8583.Utility
{
    public class Iso8583InfoGetter : IIso8583InfoGetter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Iso8583InfoGetter));
        private XElement root = null;
        private IList<BitIndex> pList = new List<BitIndex>();
        //  
        public Iso8583InfoGetter()
        {
        }

        public Iso8583InfoGetter(string posInfoUrl, string xPath)
        {
            this.setInfo(posInfoUrl);
            this.GetInfo(xPath);
        }

        private void setInfo( string posInfoUrl )
        {
            try
            {
                Assembly thisAssembly = Assembly.GetExecutingAssembly();
                Stream rgbxml = thisAssembly.GetManifestResourceStream(posInfoUrl);
                this.setInfo(rgbxml);
                rgbxml.Close();
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }
        }

        private void setInfo(Stream st)
        {
            try
            {
                this.root = XDocument.Load( st ).Root;                               
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }
        }

        private void GetInfo( string xPathStr )
        {
            XElement xtr = null;
            try
            {
                xtr = this.root.XPathSelectElement(xPathStr);
                
                xPathStr = @"./BITS";
                //log.Debug("XPATH: " + xPathStr);
                xtr = xtr.XPathSelectElement(xPathStr);
                //log.Debug("XPATH RESULT: " + xtr);
                IEnumerable<XElement> xenu = xtr.Elements("BIT");
                foreach (XElement xe in xenu)
                {
                    BitIndex pi = new BitIndex();
                    pi.Id = (int)xe.Attribute("id");
                    //pi.FixLen = (string)xe.Attribute("fixLen");
                    //pi.Type = (string)xe.Attribute("type");
                    //pi.Length = (int)xe.Attribute("len");
                    pi.Representation = (string)xe.Attribute("representation");
                    pi.Name = (string)xe.Attribute("name");
                    this.pList.Add(pi);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                //return null;
            }
            //return xtr;
        }
        
        public IList<BitIndex> GetInfos()
        {
            return this.pList;
        }

        public void ResetInfos()
        {
            this.pList.Clear();
        }        
    }
}

