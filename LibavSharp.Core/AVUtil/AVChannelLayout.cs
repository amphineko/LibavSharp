using System.Runtime.InteropServices;

namespace LibavSharp.Core.AVUtil;

public class AVChannelLayout
{
    /// <param name="channels">Channel count (i.e. nb_channels)</param>
    public static long GetDefaultChannelLayout(int channels)
    {
        return NativeMethods.GetDefaultChannelLayout(channels);
    }

    #region P/Invoke

    private static class NativeMethods
    {
        /// <remarks>
        ///     int64_t av_get_default_channel_layout(int nb_channels);
        /// </remarks>
        [DllImport(LibAVUtil.DllName,
            EntryPoint = "av_get_default_channel_layout",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern long GetDefaultChannelLayout(int channelCount);
    }

    #endregion
}