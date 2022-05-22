using System.Runtime.InteropServices;
using LibavSharp.Core.AVUtil;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVCodec;

public class AVCodecParameters : SafeHandleZeroOrMinusOneIsInvalid
{
    #region Unmanaged Fields

    public unsafe int CodecId
    {
        get => *(int*) (handle + FieldOffsets.CodecId);
        set => (*(int*) (handle + FieldOffsets.CodecId)) = value;
    }

    public unsafe AVMediaType CodecType
    {
        get => (AVMediaType) (*(int*) (handle + FieldOffsets.CodecType));
        set => *(int*) (handle + FieldOffsets.CodecType) = (int) value;
    }

    #region Unmanaged Fields: Audio-specific

    public unsafe long BitRate
    {
        get => *(long*) (handle + FieldOffsets.BitRate);
        set => (*(long*) (handle + FieldOffsets.BitRate)) = value;
    }

    public unsafe ulong ChannelLayout
    {
        get => *(ulong*) (handle + FieldOffsets.ChannelLayout);
        set => (*(ulong*) (handle + FieldOffsets.ChannelLayout)) = value;
    }

    public unsafe int Channels
    {
        get => *(int*) (handle + FieldOffsets.Channels);
        set => (*(int*) (handle + FieldOffsets.Channels)) = value;
    }

    public unsafe AVSampleFormat SampleFormat
    {
        get => (AVSampleFormat) (*(int*) (handle + FieldOffsets.Format));
        set => *(int*) (handle + FieldOffsets.Format) = (int) value;
    }

    public unsafe int SampleRate
    {
        get => *(int*) (handle + FieldOffsets.SampleRate);
        set => (*(int*) (handle + FieldOffsets.SampleRate)) = value;
    }

    #endregion

    #endregion

    #region Copy From/To Contexts

    public void CopyFromContext(AVCodecContext context)
    {
        var error = NativeMethods.ParamsFromContext(handle, context.DangerousGetHandle());
        if (error < 0) throw new LibavException(error);
    }

    public void CopyToContext(AVCodecContext context)
    {
        var error = NativeMethods.ParamsToContext(context.DangerousGetHandle(), handle);
        if (error < 0) throw new LibavException(error);
    }

    #endregion

    #region Unmanaged Resources

    public static AVCodecParameters Create()
    {
        return new AVCodecParameters(NativeMethods.Alloc(), true);
    }

    public static AVCodecParameters CreateFromContext(AVCodecContext context)
    {
        var parameters = new AVCodecParameters(NativeMethods.Alloc(), true);
        parameters.CopyFromContext(context);
        return parameters;
    }

    public static AVCodecParameters FromHandle(IntPtr handle)
    {
        return new AVCodecParameters(handle, false);
    }

    private AVCodecParameters(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        this.handle = handle;
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.Free(ref handle);
        return true;
    }

    #endregion

    #region P/Invoke & Constants

    private static class FieldOffsets
    {
        public const int CodecType = 0;

        public const int CodecId = 4;

        #region Audio-specific

        public const int BitRate = 32;

        public const int ChannelLayout = 104;

        public const int Channels = 112;

        public const int Format = 28;

        public const int SampleRate = 116;

        #endregion
    }

    private static class NativeMethods
    {
        /// <remarks>
        ///     AVCodecParameters *avcodec_parameters_alloc(void);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_parameters_alloc",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Alloc();

        /// <remarks>
        ///     void avcodec_parameters_free(AVCodecParameters **ppar);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_parameters_free",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void Free(ref IntPtr ppar);

        /// <remarks>
        ///     int avcodec_parameters_from_context(AVCodecParameters *par, const AVCodecContext *codec);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_parameters_from_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ParamsFromContext(IntPtr par, IntPtr codec);

        /// <remarks>
        ///     int avcodec_parameters_to_context(AVCodecContext *codec, const AVCodecParameters *par);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_parameters_to_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ParamsToContext(IntPtr codec, IntPtr par);
    }

    #endregion
}