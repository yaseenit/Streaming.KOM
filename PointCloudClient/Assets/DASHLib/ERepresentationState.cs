namespace KOM.DASHLib
{   
    /// <summary>
    /// A collection of states that an representation can be in.
    /// </summary>
    public enum ERepresentationState
    {
        Unbuffered,
        Buffering,
        Buffered,
        Preparing,
        Prepared,
        Playing,
        Played,
    }
}
