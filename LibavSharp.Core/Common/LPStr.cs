using System.Runtime.InteropServices;

namespace LibavSharp.Core.Common;

[StructLayout(LayoutKind.Sequential)]
public class LPStr
{
    [MarshalAs(UnmanagedType.LPStr)] public string Value;

    public static explicit operator LPStr(IntPtr handle)
    {
        return Marshal.PtrToStructure<LPStr>(handle) ?? throw new InvalidOperationException();
    }

    public static implicit operator string(LPStr str)
    {
        return str.Value;
    }

    public static implicit operator LPStr(string str)
    {
        return new LPStr {Value = str};
    }
}