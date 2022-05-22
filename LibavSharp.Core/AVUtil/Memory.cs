using System.Runtime.InteropServices;

namespace LibavSharp.Core.AVUtil;

public class Memory
{
    public static IntPtr Allocate(uint size)
    {
        var result = NativeMethods.MAlloc((UIntPtr) size);
        if (result == IntPtr.Zero) throw new OutOfMemoryException();

        return result;
    }

    public static void Free(IntPtr ptr)
    {
        NativeMethods.FreeExtern(ptr);
    }

    private static class NativeMethods
    {
        [DllImport(LibAVUtil.DllName, EntryPoint = "av_malloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MAlloc(UIntPtr size);

        [DllImport(LibAVUtil.DllName, EntryPoint = "av_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeExtern(IntPtr ptr);
    }
}