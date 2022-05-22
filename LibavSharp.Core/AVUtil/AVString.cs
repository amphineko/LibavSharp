using System.Runtime.InteropServices;
using System.Text;
using LibavSharp.Core.Common;

namespace LibavSharp.Core.AVUtil;

public class AVString
{
    public static string FromIntPtr(IntPtr ptr)
    {
        return ((LPStr) ptr).Value;
    }

    /// <param name="value">content to be initialized with</param>
    /// <param name="retainOwnership">false will never free the handle, (e.g. being passed to AVFormatContext.url)</param>
    public static IntPtr ToIntPtr(string value)
    {
        var buffer = Encoding.UTF8.GetBytes(value);
        var size = buffer.Length;

        var handle = Memory.Allocate((uint) size);
        Marshal.Copy(buffer, 0, handle, size);

        return handle;
    }
}