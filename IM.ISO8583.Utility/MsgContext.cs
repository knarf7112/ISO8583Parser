using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Kms.Utility;

namespace IM.ISO8583.Utility
{
    public class MsgContext
    {
        private LinkedListNode<IsoField> currentNode = null;
        //
        public int MsgSize { get; set; }
        public string FromTo { get; set; }
        public string Mti { get; set; }
        public string BitMap { get; set; }
        public string SrcMessage { get; set; }

        public LinkedList<IsoField> FieldList;

        public Dictionary<int, LinkedListNode<IsoField>> DicField;
        public int Start { get; set; }
        public IsoField CurrentField
        {
            get { return this.currentNode.Value; }
            set { this.currentNode.Value = value; }
        }

        public MsgContext()
        {            
            this.DicField = new Dictionary<int, LinkedListNode<IsoField>>();
            this.Start = 0;
            this.FieldList = new LinkedList<IsoField>();
            this.currentNode = this.FieldList.AddFirst(new IsoField());
            this.SrcMessage = "";
            this.MsgSize = 0;
        }
        public void AddField( int fno, string fdata )
        {
            IsoField isoField = new IsoField
            {
                FNo = fno
              , FData = fdata
              , NextFNo = -1
            };
            this.CurrentField.NextFNo = fno;
            this.currentNode = this.FieldList.AddAfter( this.currentNode, isoField );
            
            if( this.DicField.ContainsKey( fno ) )
            {
                this.DicField[fno] = this.currentNode;
            }
            else
            {
                this.DicField.Add( fno, this.currentNode );
            }
        }
        public IsoField GetField( int fno )
        {
            IsoField field = null;
            if( this.DicField.ContainsKey( fno ) )
            {
                this.currentNode = this.DicField[fno];
                field = this.CurrentField;
            }
            return field;
        }

        public bool HasField( int fno )
        {
            return "1".Equals(this.BitMap.Substring(fno - 1, 1));
        }
    }
}
