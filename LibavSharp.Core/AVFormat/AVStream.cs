using LibavSharp.Core.AVCodec;
using LibavSharp.Core.Common;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVFormat;

public class AVStream : SafeHandleZeroOrMinusOneIsInvalid
{
    #region Unmanaged Fields

    public unsafe AVCodecParameters CodecParameters
    {
        get => AVCodecParameters.FromHandle(IntPtrStruct.FromIntPtr(handle + FieldOffsets.CodecParams).Value);
        set => *(IntPtr*) (handle + FieldOffsets.CodecParams) = value.DangerousGetHandle();
    }

    public unsafe AVDiscard Discard
    {
        get => (AVDiscard) (*(int*) (handle + FieldOffsets.Discard));
        set => *(int*) (handle + FieldOffsets.Discard) = (int) value;
    }

    public unsafe int Index => *(int*) (handle + FieldOffsets.Index);

    public unsafe int TimeBaseDen
    {
        get => *(int*) (handle + FieldOffsets.TimeBaseDen);
        set => *(int*) (handle + FieldOffsets.TimeBaseDen) = value;
    }

    public unsafe int TimeBaseNum
    {
        get => *(int*) (handle + FieldOffsets.TimeBaseNum);
        set => *(int*) (handle + FieldOffsets.TimeBaseNum) = value;
    }

    #endregion

    #region Unmanaged Resources

    private static class FieldOffsets
    {
        public const int CodecParams = 208;

        public const int Discard = 52;

        public const int Index = 0;

        public const int TimeBaseDen = 20;

        public const int TimeBaseNum = 16;
    }

    /// <remarks>
    ///     AVStream allocated by avformat_new_stream should be freed by avformat_free_context.
    ///     So we don't need to free it manually and mark ownsHandle as false.
    /// </remarks>
    public AVStream(IntPtr handle) : base(false)
    {
        this.handle = handle;
    }

    protected override bool ReleaseHandle()
    {
        throw new InvalidOperationException("AVStream should be never released");
    }

    #endregion
}