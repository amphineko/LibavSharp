using System.Runtime.InteropServices;

namespace LibavSharp.Core.AVUtil;

public class LibavException : Exception
{
    public LibavException(int errorNumber) : base(GetErrorString(errorNumber))
    {
        ErrorNumber = errorNumber;
    }

    public int ErrorNumber { get; }

    #region P/Invoke

    private const int MaxErrorMessageLength = 64;

    private static unsafe string GetErrorString(int errorNumber)
    {
        var buffer = stackalloc byte[MaxErrorMessageLength];
        var error = GetErrorString(errorNumber, buffer, MaxErrorMessageLength);
        return Marshal.PtrToStringAuto((IntPtr) buffer) ??
               throw new InvalidOperationException("Failed to unmarshal error message");
    }

    /// <remarks>
    ///     int av_strerror(int errnum, char *errbuf, size_t errbuf_size);
    /// </remarks>
    [DllImport(LibAVUtil.DllName, EntryPoint = "av_strerror", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe int GetErrorString(int errnum, byte* errbuf, int errbuf_size);

    #endregion
}