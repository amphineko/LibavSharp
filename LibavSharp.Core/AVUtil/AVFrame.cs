using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVUtil;

public class AVFrame : SafeHandleZeroOrMinusOneIsInvalid
{
    #region Unmanaged Fields

    public unsafe IntPtr Data => *(IntPtr*) (handle + FieldOffsets.Data);

    public unsafe IntPtr ExtendedData => *(IntPtr*) (handle + FieldOffsets.ExtendedData);

    public unsafe long PresentationTimestamp
    {
        get => *(long*) (handle + FieldOffsets.PresentationTimestamp);
        set => (*(long*) (handle + FieldOffsets.PresentationTimestamp)) = value;
    }

    #region Unmanaged Fields: Audio-specific

    public unsafe ulong ChannelLayout
    {
        get => *(ulong*) (handle + FieldOffsets.ChannelLayout);
        set => (*(ulong*) (handle + FieldOffsets.ChannelLayout)) = value;
    }

    public unsafe int SampleCount
    {
        get => *(int*) (handle + FieldOffsets.SampleCount);
        set => (*(int*) (handle + FieldOffsets.SampleCount)) = value;
    }

    public unsafe AVSampleFormat SampleFormat
    {
        get => (AVSampleFormat) (*(int*) (handle + FieldOffsets.Format));
        set => (*(int*) (handle + FieldOffsets.Format)) = (int) value;
    }

    public unsafe int SampleRate
    {
        get => *(int*) (handle + FieldOffsets.SampleRate);
        set => (*(int*) (handle + FieldOffsets.SampleRate)) = value;
    }

    #endregion

    #endregion

    #region Allocation & Free

    public void GetBuffer()
    {
        if (Data != IntPtr.Zero)
            // TODO: also verify extended_data 
            throw new InvalidOperationException("Frame already has buffer allocated");

        var error = NativeMethods.GetBuffer(handle, 0);
        if (error != 0) throw new LibavException(error);
    }

    public AVFrame() : this(NativeMethods.Alloc(), true)
    {
        if (handle == IntPtr.Zero) throw new OutOfMemoryException("Failed to allocate AVFrame.");
    }

    private AVFrame(IntPtr handle, bool ownsHandle) : base(ownsHandle)
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
        public const int ChannelLayout = 216;

        public const int Data = 0;

        public const int ExtendedData = 96;

        public const int Format = 116;

        public const int PresentationTimestamp = 136;

        public const int SampleCount = 112;

        public const int SampleRate = 208;
    }

    private static class NativeMethods
    {
        /// <remarks>
        ///     AVFrame *av_frame_alloc(void);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_frame_alloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Alloc();

        /// <remarks>
        ///     void av_frame_free(AVFrame **frame);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_frame_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Free(ref IntPtr frame);

        /// <remarks>
        ///     int av_frame_get_buffer(AVFrame *frame, int align);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_frame_get_buffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetBuffer(IntPtr frame, int align);
    }

    #endregion
}