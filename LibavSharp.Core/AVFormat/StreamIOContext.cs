using System.Runtime.InteropServices;
using LibavSharp.Core.AVUtil;

namespace LibavSharp.Core.AVFormat;

public class StreamIOContext : AVIOContext
{
    private readonly NativeMethods.ReadPacketDelegate _readPacketCallback;
    private readonly NativeMethods.SeekDelegate _seekCallback;
    private readonly NativeMethods.WritePacketDelegate _writePacketCallback;

    private GCHandle _readPacketCallbackHandle;
    private GCHandle _seekCallbackHandle;
    private GCHandle _writePacketCallbackHandle;

    public StreamIOContext(Stream stream, bool writable) : base(IntPtr.Zero, true)
    {
        Stream = stream;

        _readPacketCallback = DoReadPacket;
        _writePacketCallback = DoWritePacket;
        _seekCallback = DoSeek;

        _readPacketCallbackHandle = GCHandle.Alloc(_readPacketCallback);
        _writePacketCallbackHandle = GCHandle.Alloc(_writePacketCallback);
        _seekCallbackHandle = GCHandle.Alloc(_seekCallback);

        const int bufferSize = 4096;
        var buffer = Memory.Allocate(bufferSize); // TODO: move this constant to a better place

        handle = NativeMethods.Allocate(buffer, bufferSize, writable ? 1 : 0, IntPtr.Zero,
            _readPacketCallback, _writePacketCallback, _seekCallback);
    }

    private Stream Stream { get; }

    protected override bool ReleaseHandle()
    {
        Flush();
        return base.ReleaseHandle();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _readPacketCallbackHandle.Free();
        _writePacketCallbackHandle.Free();
        _seekCallbackHandle.Free();
    }

    private int DoReadPacket(IntPtr _, byte[] buffer, int bufferSize)
    {
        return Stream.Read(buffer, 0, bufferSize);
    }

    private int DoWritePacket(IntPtr _, byte[] buffer, int bufferSize)
    {
        Stream.Write(new ReadOnlySpan<byte>(buffer));
        return bufferSize;
    }

    private int DoSeek(IntPtr _, long offset, AVSeekWhence whence)
    {
        if (whence == AVSeekWhence.Size) return (int) Stream.Length;

        Stream.Seek(offset, whence switch
        {
            AVSeekWhence.Set => SeekOrigin.Begin,
            AVSeekWhence.Cur => SeekOrigin.Current,
            AVSeekWhence.End => SeekOrigin.End,
            _ => throw new ArgumentOutOfRangeException(nameof(whence), whence, null)
        });

        return 0;
    }
}