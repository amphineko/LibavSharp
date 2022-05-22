using System.Runtime.InteropServices;
using LibavSharp.Core.AVUtil;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.SwResample;

public class SwrContext : SafeHandleZeroOrMinusOneIsInvalid
{
    public SwrContext(SwrContextOptions options) : base(true)
    {
        handle = NativeMethods.AllocSetOpts(IntPtr.Zero,
            options.OutputChannelLayout, options.OutputSampleFormat, options.OutputSampleRate,
            options.InputChannelLayout, options.InputSampleFormat, options.InputSampleRate,
            0, IntPtr.Zero);
        if (handle == IntPtr.Zero) throw new OutOfMemoryException("Failed to allocate SwrContext.");

        var error = NativeMethods.Init(handle);
        if (error != 0) throw new LibavException(error);
    }

    public void ConvertFrame(AVFrame? outFrame, AVFrame? inFrame)
    {
        var error = NativeMethods.ConvertFrame(handle,
            outFrame?.DangerousGetHandle() ?? IntPtr.Zero,
            inFrame?.DangerousGetHandle() ?? IntPtr.Zero);
        if (error < 0) throw new LibavException(error);
    }

    /// <param name="base">time base in which the returned delay will be</param>
    /// <seealso cref="NativeMethods.GetDelay" />
    public long GetDelay(long @base)
    {
        return NativeMethods.GetDelay(handle, @base);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.Free(ref handle);
        return true;
    }

    private static class NativeMethods
    {
        /// <remarks>
        ///     struct SwrContext *swr_alloc_set_opts(
        ///     struct SwrContext *s,
        ///     int64_t out_ch_layout, enum AVSampleFormat out_sample_fmt, int out_sample_rate,
        ///     int64_t in_ch_layout, enum AVSampleFormat in_sample_fmt, int in_sample_rate,
        ///     int log_offset, void *log_ctx)
        /// </remarks>
        [DllImport(LibSwResample.DllName,
            EntryPoint = "swr_alloc_set_opts",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AllocSetOpts(IntPtr s,
            long out_ch_layout, AVSampleFormat out_sample_fmt, int out_sample_rate,
            long in_ch_layout, AVSampleFormat in_sample_fmt, int in_sample_rate,
            int log_offset, IntPtr log_ctx);

        /// <remarks>
        ///     int swr_convert_frame(SwrContext *s, AVFrame *out, const AVFrame *in)
        /// </remarks>
        [DllImport(LibSwResample.DllName,
            EntryPoint = "swr_convert_frame",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ConvertFrame(IntPtr s, IntPtr out_frame, IntPtr in_frame);

        /// <remarks>
        ///     void swr_free(SwrContext **ss);
        /// </remarks>
        [DllImport(LibSwResample.DllName,
            EntryPoint = "swr_free",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void Free(ref IntPtr ss);

        /// <remarks>
        ///     int64_t swr_get_delay(struct SwrContext *s, int64_t base);
        /// </remarks>
        [DllImport(LibSwResample.DllName,
            EntryPoint = "swr_get_delay",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern long GetDelay(IntPtr s, long @base);

        /// <remarks>
        ///     int swr_init(struct SwrContext *s);
        /// </remarks>
        [DllImport(LibSwResample.DllName,
            EntryPoint = "swr_init",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Init(IntPtr s);
    }
}