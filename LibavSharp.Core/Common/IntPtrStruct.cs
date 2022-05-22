using System.Runtime.InteropServices;

namespace LibavSharp.Core.Common;

[StructLayout(LayoutKind.Sequential)]
public class IntPtrStruct
{
    public IntPtr Value = IntPtr.Zero;

    public static IntPtrStruct FromIntPtr(IntPtr value)
    {
        return Marshal.PtrToStructure<IntPtrStruct>(value) ?? throw new InvalidOperationException();
    }

    /// <summary>
    ///     Write the pointer value to the destination <paramref name="handle" />.
    /// </summary>
    public void ToIntPtr(IntPtr handle)
    {
        Marshal.StructureToPtr(this, handle, false);
    }
}