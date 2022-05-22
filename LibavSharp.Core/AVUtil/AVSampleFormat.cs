namespace LibavSharp.Core.AVUtil;

public enum AVSampleFormat
{
    None = -1,

    /// <summary>
    ///     unsigned 8 bits
    /// </summary>
    U8,

    /// <summary>
    ///     signed 16 bits
    /// </summary>
    S16,

    /// <summary>
    ///     signed 32 bits
    /// </summary>
    S32,

    /// <summary>
    ///     float
    /// </summary>
    FLT,

    /// <summary>
    ///     double
    /// </summary>
    DBL,

    /// <summary>
    ///     unsigned 8 bits, planar
    /// </summary>
    U8P,

    /// <summary>
    ///     signed 16 bits, planar
    /// </summary>
    S16P,

    /// <summary>
    ///     signed 32 bits, planar
    /// </summary>
    S32P,

    /// <summary>
    ///     float, planar
    /// </summary>
    FLTP,

    /// <summary>
    ///     double, planar
    /// </summary>
    DBLP,

    /// <summary>
    ///     signed 64 bits
    /// </summary>
    S64,

    /// <summary>
    ///     signed 64 bits, planar
    /// </summary>
    S64P,

    /// <summary>
    ///     Number of sample formats. DO NOT USE if linking dynamically
    /// </summary>
    NB
}