using System.Runtime.InteropServices;
using LibavSharp.Core.AVUtil;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVCodec;

public class AVCodecContext : SafeHandleZeroOrMinusOneIsInvalid
{
    #region General

    public void Open(AVDictionary? options = null)
    {
        var optionsHandle = options?.DangerousGetHandle() ?? IntPtr.Zero;
        var error = NativeMethods.Open(handle, IntPtr.Zero, ref optionsHandle);

        if (options is { } && options.DangerousGetHandle() != optionsHandle)
            // update the dictionary wrapper with returned dictionary
            options.DangerousSetHandle(optionsHandle);

        if (options?.Count > 0) throw new ArgumentException($"Unused options found: {options}");

        if (error != 0) throw new LibavException(error);
    }

    #endregion

    private static class FieldOffsets
    {
        public const int CodecType = 12;

        public const int CodecId = 24;

        public const int FrameSize = 364;

        public const int StandardCompliance = 508;

        public const int TimeBaseDen = 104;

        public const int TimeBaseNum = 100;

        // Audio-specific

        public const int ChannelLayout = 384;

        public const int SampleFormat = 360;

        public const int SampleRate = 352;
    }

    private static class NativeMethods
    {
        /// <remarks>
        ///     AVCodecContext *avcodec_alloc_context3(const AVCodec *codec);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_alloc_context3",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AllocContext(IntPtr codec);

        /// <remarks>
        ///     void avcodec_free_context(AVCodecContext **pavctx);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_free_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeContext(ref IntPtr context);

        /// <remarks>
        ///     int avcodec_open2(AVCodecContext *avctx, const AVCodec *codec, AVDictionary **options);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_open2",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Open(IntPtr context, IntPtr codec, ref IntPtr options);

        /// <remarks>
        ///     int avcodec_receive_frame(AVCodecContext *avctx, AVFrame *frame);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_receive_frame",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReceiveFrame(IntPtr context, IntPtr frame);

        /// <remarks>
        ///     int avcodec_receive_packet(AVCodecContext *avctx, AVPacket *avpkt);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_receive_packet",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReceivePacket(IntPtr context, IntPtr packet);

        /// <remarks>
        ///     int avcodec_send_frame(AVCodecContext *avctx, const AVFrame *frame);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_send_frame",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int SendFrame(IntPtr context, IntPtr frame);

        /// <remarks>
        ///     int avcodec_send_packet(AVCodecContext *avctx, const AVPacket *avpkt);
        /// </remarks>
        [DllImport(LibAVCodec.DllName,
            EntryPoint = "avcodec_send_packet",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int SendPacket(IntPtr context, IntPtr packet);
    }

    #region Decoding

    /// <returns>null if no frame is available (e.g. more input packets required), otherwise the frame</returns>
    public AVFrame? ReceiveFrame()
    {
        var frame = new AVFrame();
        try
        {
            if (ReceiveFrame(frame)) return frame;

            frame.Close();
            return null;
        }
        catch (Exception)
        {
            frame.Close();
            throw;
        }
    }

    public void SendPacket(AVPacket? packet)
    {
        var error = NativeMethods.SendPacket(handle, packet?.DangerousGetHandle() ?? IntPtr.Zero);

        if (error == Errors.Again) throw new InvalidOperationException("Input is not accepted in the current state.");

        if (error == Errors.EndOfFile) throw new EndOfStreamException("AVCodecContext has been flushed.");

        if (error != 0) throw new LibavException(error);
    }

    /// <returns>true if a frame is decoded, false if output is not available yet (e.g. more packets required)</returns>
    private bool ReceiveFrame(AVFrame frame)
    {
        var error = NativeMethods.ReceiveFrame(handle, frame.DangerousGetHandle());
        if (error == Errors.Again) return false;

        return error switch
        {
            0 => true,
            Errors.EndOfFile => throw new EndOfStreamException("Decoder has been fully flushed."),
            _ => throw new LibavException(error)
        };
    }

    #endregion

    #region Encoding

    public AVPacket? ReceivePacket()
    {
        var packet = new AVPacket();
        try
        {
            if (ReceivePacket(packet)) return packet;

            packet.Close();
            return null;
        }
        catch (Exception)
        {
            packet.Close();
            throw;
        }
    }

    public void SendFrame(AVFrame? frame)
    {
        var error = NativeMethods.SendFrame(handle, frame?.DangerousGetHandle() ?? IntPtr.Zero);

        // TODO: maybe catch EAGAIN here?

        if (error == Errors.EndOfFile) throw new EndOfStreamException("Encoder has been flushed.");

        if (error != 0) throw new LibavException(error);
    }

    private bool ReceivePacket(AVPacket packet)
    {
        var error = NativeMethods.ReceivePacket(handle, packet.DangerousGetHandle());
        if (error == Errors.Again) return false;

        return error switch
        {
            0 => true,
            Errors.EndOfFile => throw new EndOfStreamException("Encoder has been fully flushed."),
            _ => throw new LibavException(error)
        };
    }

    #endregion

    #region Unmanaged Fields

    public unsafe AVMediaType CodecType => (AVMediaType) (*(int*) (handle + FieldOffsets.CodecType));

    public unsafe int CodecId => *(int*) (handle + FieldOffsets.CodecId);

    public unsafe int FrameSize => *(int*) (handle + FieldOffsets.FrameSize);

    public unsafe StandardCompliance StandardCompliance
    {
        get => (StandardCompliance) (*(int*) (handle + FieldOffsets.StandardCompliance));
        set => *(int*) (handle + FieldOffsets.StandardCompliance) = (int) value;
    }

    #region Unmanaged Fields: Audio-specific

    public unsafe ulong ChannelLayout => *(ulong*) (handle + FieldOffsets.ChannelLayout);

    public unsafe AVSampleFormat SampleFormat => (AVSampleFormat) (*(int*) (handle + FieldOffsets.SampleFormat));

    public unsafe int SampleRate => *(int*) (handle + FieldOffsets.SampleRate);

    public unsafe int TimeBaseDen => *(int*) (handle + FieldOffsets.TimeBaseDen);

    public unsafe int TimeBaseNum => *(int*) (handle + FieldOffsets.TimeBaseNum);

    #endregion

    #endregion

    #region Context Allocation & Free

    public AVCodecContext(AVCodec codec) : this(NativeMethods.AllocContext(codec.DangerousGetHandle()), true)
    {
    }

    private AVCodecContext(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        this.handle = handle;
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.FreeContext(ref handle);
        return true;
    }

    #endregion
}