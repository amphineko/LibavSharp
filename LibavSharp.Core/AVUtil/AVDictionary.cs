using System.Runtime.InteropServices;
using LibavSharp.Core.Common;
using Microsoft.Win32.SafeHandles;

namespace LibavSharp.Core.AVUtil;

public class AVDictionary : SafeHandleZeroOrMinusOneIsInvalid
{
    private AVDictionary(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        this.handle = handle;
    }

    public AVDictionary() : this(IntPtr.Zero, true)
    {
    }

    public int Count => NativeMethods.Count(handle);

    public AVDictionary Copy()
    {
        var newDict = IntPtr.Zero;
        var error = NativeMethods.Copy(ref newDict, handle, 0);
        if (error < 0) throw new LibavException(error);

        return new AVDictionary(newDict, true);
    }

    public void Set(string key, string value)
    {
        var error = NativeMethods.Set(ref handle, key, value, 0);
        if (error < 0) throw new LibavException(error);
    }

    public override string ToString()
    {
        var buffer = IntPtr.Zero;
        try
        {
            var error = NativeMethods.GetString(handle, ref buffer, "=", ",");
            if (error < 0) throw new LibavException(error);

            return ((LPStr) buffer).Value;
        }
        finally
        {
            if (buffer != IntPtr.Zero) Memory.Free(buffer);
        }
    }

    /// <summary>
    ///     Dangerously updates the handle to point to a new dictionary.
    ///     Only use this if you know what you are doing (e.g. after calling avcodec_open2()).
    /// </summary>
    public void DangerousSetHandle(IntPtr newHandle)
    {
        handle = newHandle;
    }

    protected override bool ReleaseHandle()
    {
        if (handle != IntPtr.Zero) NativeMethods.Free(ref handle);

        return true;
    }

    private static class NativeMethods
    {
        /// <remarks>
        ///     int av_dict_count(const AVDictionary *m);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_dict_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Count(IntPtr m);

        /// <remarks>
        ///     int av_dict_copy(AVDictionary **dst, const AVDictionary *src, int flags);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_dict_copy", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Copy(ref IntPtr dst, IntPtr src, int flags);

        /// <remarks>
        ///     void av_dict_free(AVDictionary **pm);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_dict_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Free(ref IntPtr pm);

        /// <remarks>
        ///     int av_dict_get_string(const AVDictionary *m, char **buffer, const char key_val_sep, const char pairs_sep);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_dict_get_string", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetString(IntPtr m, ref IntPtr buffer, string key_val_sep, string pairs_sep);

        /// <remarks>
        ///     int av_dict_set(AVDictionary **pm, const char *key, const char *value, int flags);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_dict_set", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Set(
            ref IntPtr pm,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string key,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string value,
            int flags);

        /// <remarks>
        ///     int av_dict_set_int(AVDictionary **pm, const char *key, int64_t value, int flags);
        /// </remarks>
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_dict_set_int", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetInt(
            ref IntPtr pm,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string key,
            long value,
            int flags);
    }
}