using System;
using System.Collections.Generic;
//
using Kms.Utility;
using Kms.Crypto;
//
using Common.Logging;

namespace IM.ISO8583.Utility
{ 
    public class BitWorker : IBitWorker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BitWorker));
        //IDictionary<int, BitIndex> bitDic = new Dictionary<int, BitIndex>();
        List<BitIndex> bitIndexList = new List<BitIndex>();
        //
        //public string Bits { set; private get; }
        //public IBitMapper BitMapper { set; private get; }
        public BitWorker(IIso8583InfoGetter infoGetter)
        {
            this.setBitList( infoGetter.GetInfos() );
        }

        public BitWorker()
        {
        }

        public int Length()
        {
            return bitIndexList.Count;
        }

        // Indexer
        public BitIndex this[int index]
        {
            get { return bitIndexList[index]; }
            set
            {
                //log.Debug(index + ":" + value);
                try
                {
                    if (index < bitIndexList.Count)
                    {
                        bitIndexList.RemoveAt(index);
                        //bitDic.Remove( value.Id );
                    }
                    value.PatternWorker = PatternFactory.GetInstance().GetPattern(value);
                    bitIndexList.Insert(index, value);
                    //bitDic.Add( value.Id, value );
                }
                catch (Exception ex)
                {
                    log.Error(ex.StackTrace);
                }
            }
        }

        private void setBitList(IList<BitIndex> bitList)
        {
            //log.Debug(tList.Count);
            for (int i = 0; i < bitList.Count; i++)
            {
                int idx = bitList[i].Id;
                this[idx] = bitList[i];
            }
        }

        public BitIndex Get( int fno )
        {
            return this[fno];
        }
    }
}

