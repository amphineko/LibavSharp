using System.Runtime.InteropServices;
using LibavSharp.Core.Common;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVFormat;

public class AVIOContext : SafeHandleZeroOrMinusOneIsInvalid
{
    #region Unmanaged Fields

    protected IntPtr Buffer => IntPtrStruct.FromIntPtr(handle + FieldOffsets.Buffer).Value;

    #endregion

    #region General

    protected void Flush()
    {
        NativeMethods.Flush(handle);
    }

    #endregion

    #region Allocation & Deallocation

    protected AVIOContext(IntPtr handle, bool ownsHandle) : base(ownsHandle)
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
        public const int Buffer = 8;
    }

    protected static class NativeMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ReadPacketDelegate(IntPtr _,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer, int bufferSize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int SeekDelegate(IntPtr _, long offset, [MarshalAs(UnmanagedType.I4)] AVSeekWhence whence);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int WritePacketDelegate(IntPtr _,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
            byte[] buffer, int bufferSize);

        /// <remarks>
        ///     AVIOContext *avio_alloc_context(
        ///     unsigned char *buffer,
        ///     int buffer_size,
        ///     int write_flag,
        ///     void *opaque,
        ///     int (*read_packet)(void *opaque, uint8_t *buf, int buf_size),
        ///     int (*write_packet)(void *opaque, uint8_t *buf, int buf_size),
        ///     int64_t (*seek)(void *opaque, int64_t offset, int whence))
        /// </remarks>
        [DllImport(LibAVFormat.DllName, EntryPoint = "avio_alloc_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Allocate(IntPtr buffer, int bufferSize, int writeFlag, IntPtr opaque,
            ReadPacketDelegate readPacket,
            WritePacketDelegate writePacket,
            SeekDelegate seek);

        /// <remarks>
        ///     void avio_flush(AVIOContext *s);
        /// </remarks>
        [DllImport(LibAVFormat.DllName, EntryPoint = "avio_flush", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Flush(IntPtr s);

        /// <remarks>
        ///     void avio_context_free(AVIOContext **ps);
        /// </remarks>
        [DllImport(LibAVFormat.DllName, EntryPoint = "avio_context_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Free(ref IntPtr ps);
    }

    #endregion
}