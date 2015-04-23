using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//

using Common.Logging;

namespace IM.ISO8583.Utility
{   
    /// <summary>
    /// The ISO8583 pattern 'FlyweightFactory' class
    /// </summary>
    public class PatternFactory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PatternFactory));
        private static PatternFactory patternFactory = new PatternFactory();
        //
        private IDictionary<string, IPattern> dicPatterns = new Dictionary<string, IPattern>();
        private object obj = new object();

        // Constructor
        private PatternFactory()
        {
        }

        public static PatternFactory GetInstance()
        {
            return patternFactory;
        }

        public IPattern GetPattern( BitIndex bitIndex )
        {
            lock (this.obj)
            {
                if (! this.dicPatterns.ContainsKey( bitIndex.Representation ) )
                {
                    //log.Debug( "New Pattern: " + bitIndex.Representation );
                    IPattern pattern = getPattern(bitIndex);
                    this.dicPatterns.Add( bitIndex.Representation, pattern );
                }
            }
            return this.dicPatterns[bitIndex.Representation];
        }

        private IPattern getPattern( BitIndex bitIndex )
        {
            string reSrc = bitIndex.Representation;
            //
            string rePattern = @"^(?'TYPE'\S+?)\s+(?'LEN'\d+?)$";
            string type;
            int length;
            int variLen;
            //
            Match m = Regex.Match ( reSrc, rePattern );
            // fixed field
            if( m.Success )
            {
               type = (m.Groups["TYPE"]).Value;
               length = Convert.ToInt32( (m.Groups["LEN"]).Value, 10 );
               if( "n".Equals(type) || "x+n".Equals(type) )
               {
                   return new NumFixPattern( length );
               }
               else if( "b".Equals( type ))
               {
                   return new StrFixPattern( length / 8 * 2 );
               }
               else 
               {
                   return new StrFixPattern( length );
               }
            }
            rePattern = @"^(?'TYPE'\S+?)\s+(?'VAR'\.+?)(?'LEN'\d+?)$";
            m = Regex.Match ( reSrc, rePattern );
            if( m.Success )
            {
               type = (m.Groups["TYPE"]).Value;
               variLen = (m.Groups["VAR"].Value).Length;
               length = Convert.ToInt32( (m.Groups["LEN"]).Value, 10 );
               return new VariPattern( variLen, length );
            }
            //
            rePattern = @"\s+(?'LEN'\d+?)$";
            m = Regex.Match(reSrc, rePattern);
            if (m.Success)
            {
                length = Convert.ToInt32((m.Groups["LEN"]).Value, 10);
                return new StrFixPattern(length);
            }
            return null;
        }
    }
}
