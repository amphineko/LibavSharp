using System.Threading.Channels;
using LibavSharp.Core.AVCodec;
using LibavSharp.Core.AVUtil;
using LibavSharp.Core.SwResample;

namespace LibavSharp.Extensions.Resampling;

public class ResampleFramer
{
    public ResampleFramer(ChannelWriter<AVFrame> resampledFrames, ChannelReader<AVFrame> sourceFrames,
        AVCodecContext encoder)
    {
        ResampledFrames = resampledFrames;
        SourceFrames = sourceFrames;
        Encoder = encoder;
    }

    private AVCodecContext Encoder { get; }

    private long LastPresentationTimestamp { get; set; }

    private ChannelWriter<AVFrame> ResampledFrames { get; }

    private ChannelReader<AVFrame> SourceFrames { get; }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        // peek a frame to configure SwrContext

        if (!await SourceFrames.WaitToReadAsync(cancellationToken)) return 0;

        if (!SourceFrames.TryPeek(out var peek) && peek is not { }) return 0;

        var ctx = new SwrContext(new SwrContextOptions
        {
            InputChannelLayout = (long) peek.ChannelLayout,
            InputSampleRate = peek.SampleRate,
            InputSampleFormat = peek.SampleFormat,
            OutputChannelLayout = (long) Encoder.ChannelLayout,
            OutputSampleRate = Encoder.SampleRate,
            OutputSampleFormat = Encoder.SampleFormat
        });

        var frameCount = 0;

        while (await SourceFrames.WaitToReadAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            // enqueue inbound frames

            using var sourceFrame = await SourceFrames.ReadAsync(cancellationToken);
            ctx.ConvertFrame(null, sourceFrame);

            // dequeue frames when enough samples are available to fit the encoder's frame size

            while (true)
            {
                var samples = await EmitFrame(false, ctx, cancellationToken);
                if (samples == 0) break;

                frameCount++;
            }
        }

        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        if (await EmitFrame(true, ctx, cancellationToken) > 0)
        {
            // flush remaining samples into a frame
            ++frameCount;
        }

        ResampledFrames.Complete();

        return frameCount;
    }

    private async Task<ulong> EmitFrame(bool flush, SwrContext ctx, CancellationToken cancellationToken)
    {
        var delay = ctx.GetDelay(Encoder.SampleRate);
        var encoderFrameSize = Encoder.FrameSize;

        if (delay == 0 || delay < encoderFrameSize && !flush) return 0;

        var actualFrameSize = encoderFrameSize > 0 ? Math.Min(encoderFrameSize, delay) : delay;
        await EmitFrame(actualFrameSize, ctx, cancellationToken);

        return (ulong) actualFrameSize;
    }

    private async Task EmitFrame(long sampleCount, SwrContext ctx, CancellationToken cancellationToken)
    {
        var frame = new AVFrame();
        frame.ChannelLayout = Encoder.ChannelLayout;
        frame.SampleCount = (int) sampleCount;
        frame.SampleFormat = Encoder.SampleFormat;
        frame.SampleRate = Encoder.SampleRate;
        frame.GetBuffer();

        ctx.ConvertFrame(frame, null);

        frame.PresentationTimestamp = LastPresentationTimestamp + sampleCount;
        LastPresentationTimestamp += sampleCount;
        await ResampledFrames.WriteAsync(frame, cancellationToken);
    }
}