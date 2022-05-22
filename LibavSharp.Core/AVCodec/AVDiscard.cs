namespace LibavSharp.Core.AVCodec;

public enum AVDiscard
{
    /// <summary> 
    /// discard nothing
    /// </summary>
    None = -16,

    /// <summary> 
    /// discard useless packets like 0 size packets in avi
    /// </summary>
    Default = 0,

    /// <summary> 
    /// discard all non reference
    /// </summary>
    NonReference = 8,

    /// <summary> 
    /// discard all bidirectional frames
    /// </summary>
    Bidirectional = 16,

    /// <summary> 
    /// discard all non intra frames
    /// </summary>
    Nonintra = 24,

    /// <summary> 
    /// discard all frames except keyframes
    /// </summary>
    Nonkey = 32,

    /// <summary> 
    /// discard all
    /// </summary>
    All = 48,
}