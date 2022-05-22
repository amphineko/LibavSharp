using LibavSharp.Core.AVUtil;

namespace LibavSharp.Core.SwResample;

public class SwrContextOptions
{
    public long OutputChannelLayout { get; set; }

    public AVSampleFormat OutputSampleFormat { get; set; }

    public int OutputSampleRate { get; set; }

    public long InputChannelLayout { get; set; }

    public AVSampleFormat InputSampleFormat { get; set; }

    public int InputSampleRate { get; set; }
}