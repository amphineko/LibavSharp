using System.Runtime.InteropServices;
using LibavSharp.Core.AVCodec;
using LibavSharp.Core.AVUtil;
using LibavSharp.Core.Common;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVFormat;

public class AVFormatContext : SafeHandleZeroOrMinusOneIsInvalid
{
    #region Demuxing

    public static AVFormatContext OpenInput(string url)
    {
        var handle = IntPtr.Zero;
        var options = IntPtr.Zero;
        var error = NativeMethods.OpenInput(ref handle, url, IntPtr.Zero, ref options);
        if (error < 0) throw new LibavException(error);

        return new AVFormatContext(handle, true, true);
    }

    public AVStream? FindBestOrFirstStream(AVMediaType type)
    {
        try
        {
            var stream = FindBestStream(type);
            return stream;
        }
        catch (LibavException e)
        {
            if (e.ErrorNumber != Errors.StreamNotFound) throw;

            var stream = FindFirstStream(type);
            return stream;
        }
    }

    public AVStream FindBestStream(AVMediaType type)
    {
        return FindBestStream(type, out _);
    }

    public void ReadFrame(AVPacket packet)
    {
        var error = NativeMethods.ReadFrame(handle, packet);

        if (error == Errors.EndOfFile)
            throw new EndOfStreamException("AVFormatContext has reached the end of the file.");

        if (error != 0) throw new LibavException(error);
    }

    private AVStream FindBestStream(AVMediaType type, out AVCodec.AVCodec decoderCodec)
    {
        var result = NativeMethods.FindBestStream(handle, type, -1, -1, out var decoderHandle, 0);
        if (result < 0) throw new LibavException(result);

        decoderCodec = AVCodec.AVCodec.FromHandle(decoderHandle);
        return GetStream(result);
    }

    private AVStream? FindFirstStream(AVMediaType type)
    {
        for (var i = 0; i < StreamCount; ++i)
        {
            var stream = GetStream(i);
            if (stream.CodecParameters.CodecType == type)
            {
                return stream;
            }
        }

        return null;
    }

    #endregion

    #region Muxing

    public static AVFormatContext OpenOutput(AVIOContext ioContext, AVOutputFormat outputFormat)
    {
        var format = new AVFormatContext(NativeMethods.AllocContext(), false, true);
        format.IOContext = ioContext.DangerousGetHandle();
        format.OutputFormat = outputFormat.DangerousGetHandle();
        return format;
    }

    public AVStream NewStream(AVCodec.AVCodec codec)
    {
        return new AVStream(NativeMethods.NewStream(handle, codec.DangerousGetHandle()));
    }

    public void WriteFrameInterleaved(AVPacket packet)
    {
        var error = NativeMethods.InterleavedWriteFrame(handle, packet.DangerousGetHandle());
        if (error < 0) throw new LibavException(error);
    }

    public void WriteHeader()
    {
        var options = IntPtr.Zero;
        var error = NativeMethods.WriteHeader(handle, ref options);
        if (error < 0) throw new LibavException(error);
    }

    public void WriteTrailer()
    {
        var error = NativeMethods.WriteTrailer(handle);
        if (error < 0) throw new LibavException(error);
    }

    #endregion

    #region Unmanaged Fields

    public unsafe int FormatProbeScore => *(int*) (handle + FieldOffsets.FormatProbeScore);

    public unsafe uint StreamCount => *(uint*) (handle + FieldOffsets.StreamCount);

    public unsafe string? Url
    {
        get => AVString.FromIntPtr(handle + FieldOffsets.Url);
        set
        {
            var oldHandle = *(IntPtr*) (handle + FieldOffsets.Url);
            if (oldHandle != IntPtr.Zero)
                Memory.Free(oldHandle);

            *(IntPtr*) (handle + FieldOffsets.Url) = value is { } ? AVString.ToIntPtr(value) : IntPtr.Zero;
        }
    }

    private IntPtr IOContext
    {
        get => IntPtrStruct.FromIntPtr(handle + FieldOffsets.IOContext).Value;
        set => new IntPtrStruct {Value = value}.ToIntPtr(handle + FieldOffsets.IOContext);
    }

    private IntPtr OutputFormat
    {
        get => IntPtrStruct.FromIntPtr(handle + FieldOffsets.OutputFormat).Value;
        set => new IntPtrStruct {Value = value}.ToIntPtr(handle + FieldOffsets.OutputFormat);
    }

    public unsafe AVStream GetStream(int index)
    {
        var array0 = (IntPtr) (*(void**) (handle + FieldOffsets.StreamArrayBase));
        return new AVStream(IntPtrStruct.FromIntPtr(array0 + index * IntPtr.Size).Value);
    }

    #endregion

    #region Allocation & Deallocation

    private AVFormatContext(IntPtr handle, bool shouldCloseInput, bool ownsHandle) : base(ownsHandle)
    {
        this.handle = handle;
        _shouldCloseInput = shouldCloseInput;
    }

    protected override bool ReleaseHandle()
    {
        if (_shouldCloseInput)
            NativeMethods.CloseInput(ref handle);
        else
            NativeMethods.FreeContext(handle);

        return true;
    }

    private readonly bool _shouldCloseInput;

    static AVFormatContext()
    {
        var error = NativeMethods.NetworkInit();
        if (error < 0) throw new LibavException(error);
    }

    #endregion

    #region P/Invoke & Constants

    private static class FieldOffsets
    {
        public const int FormatProbeScore = 300;

        public const int IOContext = 32;

        public const int OutputFormat = 16;

        public const int StreamArrayBase = 48;

        public const int StreamCount = 44;

        public const int Url = 56;
    }

    private static class NativeMethods
    {
        /// <remarks>
        ///     AVFormatContext *avformat_alloc_context(void);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "avformat_alloc_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AllocContext();

        /// <remarks>
        ///     void avformat_close_input(AVFormatContext **ps);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "avformat_close_input",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseInput(ref IntPtr ps);

        /// <remarks>
        ///     int av_find_best_stream(AVFormatContext *ic, enum AVMediaType type, int wanted_stream_nb, int related_stream,
        ///     AVCodec **decoder_ret, int flags);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "av_find_best_stream",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int FindBestStream(IntPtr ic, AVMediaType type, int wantedStreamNb,
            int relatedStream, out IntPtr decoderRet, int flags);

        /// <remarks>
        ///     void avformat_free_context(AVFormatContext *s);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "avformat_free_context",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeContext(IntPtr s);

        /// <remarks>
        /// int avformat_network_init(void);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "avformat_network_init",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int NetworkInit();

        /// <remarks>
        ///     int av_interleaved_write_frame(AVFormatContext *s, AVPacket *pkt);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "av_interleaved_write_frame",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int InterleavedWriteFrame(IntPtr s, IntPtr pkt);

        /// <remarks>
        ///     AVStream *avformat_new_stream(AVFormatContext *s, const AVCodec *c);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "avformat_new_stream",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NewStream(IntPtr s, IntPtr c);

        /// <remarks>
        ///     int avformat_open_input(AVFormatContext **ps, const char *filename, AVInputFormat *fmt, AVDictionary **options)
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "avformat_open_input",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenInput(ref IntPtr ps, string filename, IntPtr fmt, ref IntPtr options);

        /// <remarks>
        ///     int av_read_frame(AVFormatContext *s, AVPacket *pkt);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "av_read_frame",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReadFrame(IntPtr s, AVPacket pkt);

        /// <remarks>
        ///     int avformat_write_header(AVFormatContext *s, AVDictionary **options)
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "avformat_write_header",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int WriteHeader(IntPtr s, ref IntPtr options);

        /// <remarks>
        ///     int av_write_trailer(AVFormatContext *s);
        /// </remarks>
        [DllImport(LibAVFormat.DllName,
            EntryPoint = "av_write_trailer",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int WriteTrailer(IntPtr s);
    }

    #endregion
}