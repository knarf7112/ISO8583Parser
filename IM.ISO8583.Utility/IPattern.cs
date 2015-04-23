namespace IM.ISO8583.Utility
{
    /// <summary>
    /// The 'Flyweight' interface class
    /// </summary>
    public interface IPattern
    {
        void Build( MsgContext buildState );
        void Parse( MsgContext parseState );
    }
}
