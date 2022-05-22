using System.Runtime.InteropServices;
using LibavSharp.Core.AVUtil;
using LibavSharp.Core.Common;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVCodec;

public class AVCodec : SafeHandleZeroOrMinusOneIsInvalid
{
    #region Unmanaged Fields

    public string Name => (LPStr) (handle + FieldOffsets.Name);

    public string LongName => (LPStr) (handle + FieldOffsets.LongName);

    public unsafe IReadOnlyList<AVSampleFormat> GetSupportedSampleFormats()
    {
        var formats = new List<AVSampleFormat>();

        var format0 = *(int**) (handle + FieldOffsets.SampleFormats);
        for (var i = format0; *i != (int) AVSampleFormat.None; ++i) formats.Add((AVSampleFormat) (*i));

        return formats;
    }

    #endregion

    #region Factories

    public static AVCodec FindDecoder(int codecId)
    {
        return new AVCodec(Extern.FindDecoder(codecId));
    }

    public static AVCodec FindEncoder(int codecId)
    {
        return new AVCodec(Extern.FindEncoder(codecId));
    }

    public static AVCodec FromHandle(IntPtr inHandle)
    {
        return new AVCodec(inHandle);
    }

    private AVCodec(IntPtr handle) : base(false)
    {
        this.handle = handle;
    }

    protected override bool ReleaseHandle()
    {
        throw new InvalidOperationException("AVCodec instances are owned by the library and should never be released.");
    }

    #endregion

    #region P/Invoke & Constants

    private static class FieldOffsets
    {
        public const int Name = 0;

        public const int LongName = 8;

        // Audio-specific

        public const int SampleFormats = 56;
    }

    private static class Extern
    {
        /// <remarks>
        ///     AVCodec *avcodec_find_decoder(enum AVCodecID id);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_find_decoder",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr FindDecoder(int id);

        /// <remarks>
        ///     AVCodec *avcodec_find_encoder(enum AVCodecID id);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_find_encoder",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr FindEncoder(int id);
    }

    #endregion
}