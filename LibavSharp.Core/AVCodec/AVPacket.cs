using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVCodec;

public class AVPacket : SafeHandleZeroOrMinusOneIsInvalid
{
    #region Reading

    public void CopyTo(byte[] buffer)
    {
        if (buffer.Length < Size) throw new ArgumentException("Buffer size is too small");

        Marshal.Copy(Data, buffer, 0, Size);
    }

    public unsafe void CopyTo(Stream stream)
    {
        var data = *(byte**) (handle + FieldOffsets.Data);

        if (data == null) throw new InvalidOperationException("Packet data is null");

        stream.Write(new ReadOnlySpan<byte>(data, Size));
    }

    #endregion

    #region Unmanaged Fields

    public unsafe long PresentationTimestamp
    {
        get => *(long*) (handle + FieldOffsets.PresentationTimestamp);
        set => (*(long*) (handle + FieldOffsets.PresentationTimestamp)) = value;
    }

    public unsafe long DecompressionTimestamp
    {
        get => *(long*) (handle + FieldOffsets.DecompressionTimestamp);
        set => (*(long*) (handle + FieldOffsets.DecompressionTimestamp)) = value;
    }

    public unsafe int StreamIndex
    {
        get => *(int*) (handle + FieldOffsets.StreamIndex);
        set => (*(int*) (handle + FieldOffsets.StreamIndex)) = value;
    }

    public unsafe IntPtr Data
    {
        get => *(IntPtr*) (handle + FieldOffsets.Data);
        set => (*(IntPtr*) (handle + FieldOffsets.Data)) = value;
    }

    public unsafe int Size
    {
        get => *(int*) (handle + FieldOffsets.Size);
        set => (*(int*) (handle + FieldOffsets.Size)) = value;
    }

    #endregion

    #region Unmanaged Resources

    public AVPacket Clone()
    {
        return new AVPacket(Extern.Clone(handle), true);
    }

    public AVPacket() : this(Extern.Alloc(), true)
    {
    }

    private AVPacket(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        this.handle = handle;
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid) Extern.Free(ref handle);

        return true;
    }

    #endregion

    #region P/Invoke & Constants

    private static class FieldOffsets
    {
        public const int Data = 24;

        public const int DecompressionTimestamp = 16;

        public const int PresentationTimestamp = 8;

        public const int Size = 32;

        public const int StreamIndex = 36;
    }

    private static class Extern
    {
        /// <remarks>
        ///     AVPacket *av_packet_alloc(void);
        /// </remarks>
        [DllImport(LibAVCodec.DllName, EntryPoint = "av_packet_alloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Alloc();

        /// <remarks>
        ///     AVPacket *av_packet_clone(const AVPacket *src);
        /// </remarks>
        [DllImport(LibAVCodec.DllName, EntryPoint = "av_packet_clone", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Clone(IntPtr src);

        /// <remarks>
        ///     void av_packet_free(AVPacket **pkt);
        /// </remarks>
        [DllImport(LibAVCodec.DllName, EntryPoint = "av_packet_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Free(ref IntPtr pkt);
    }

    #endregion
}