using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVFormat;

public class AVOutputFormat : SafeHandleZeroOrMinusOneIsInvalid
{
    private AVOutputFormat(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        this.handle = handle;
    }

    public static AVOutputFormat GuessFormat(string? shortName = null, string? filename = null, string? mimeType = null)
    {
        var result = NativeMethods.GuessFormat(shortName, filename, mimeType);
        if (result == IntPtr.Zero) throw new ArgumentException("Cannot find output format for given parameters");

        return new AVOutputFormat(result, false);
    }

    protected override bool ReleaseHandle()
    {
        throw new InvalidOperationException("AVOutputFormats are owned by the library and should never be released");
    }

    #region P/Invoke

    private static class NativeMethods
    {
        /// <remarks>
        ///     const AVOutputFormat *av_guess_format(const char *short_name, const char *filename, const char *mime_type);
        /// </remarks>
        [DllImport(LibAVFormat.DllName, EntryPoint = "av_guess_format", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GuessFormat(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? shortName,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? filename,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? mimeType);
    }

    #endregion
}