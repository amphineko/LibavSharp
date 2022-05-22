using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVUtil;

public class AVBufferRef : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <remarks>
    ///     For circumstances where you need to copy the data out of the buffer,
    ///     a new reference to the buffer should be created with <see cref="Ref" />.
    ///     Otherwise, the reference will probably be lost when the buffer is being copied.
    /// </remarks>
    public AVBufferRef(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        this.handle = handle;
    }

    public unsafe void CopyTo(Stream stream)
    {
        var span = new ReadOnlySpan<byte>(*(byte**) (handle + FieldOffsets.Data), *(int*) (handle + FieldOffsets.Size));
        stream.Write(span);
    }

    public AVBufferRef Ref()
    {
        if (handle == IntPtr.Zero) throw new ObjectDisposedException("AVBufferRef");

        return Ref(this);
    }

    private static AVBufferRef Ref(AVBufferRef buf)
    {
        var ptr = NativeMethods.Ref(buf.DangerousGetHandle());
        return new AVBufferRef(ptr, true);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.Unref(ref handle);
        return true;
    }

    private static class FieldOffsets
    {
        public const int Data = 8;

        public const int Size = 16;
    }

    private static class NativeMethods
    {
        /// <remarks>
        ///     AVBufferRef *av_buffer_ref(AVBufferRef *buf);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_buffer_ref", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Ref(IntPtr buf);

        /// <remarks>
        ///     void av_buffer_unref(AVBufferRef **buf);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_buffer_unref", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Unref(ref IntPtr buf);
    }
}