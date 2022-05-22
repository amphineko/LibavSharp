using System.Threading.Channels;
using LibavSharp.Core.AVCodec;
using LibavSharp.Core.AVFormat;
using LibavSharp.Core.AVUtil;

namespace LibavSharp.Extensions.Demuxing;

public static class StreamDecoder
{
    public static async Task<int> DecodeFramesAsync(ChannelWriter<AVFrame> frames, ChannelReader<AVPacket> packets,
        AVStream stream, CancellationToken cancellationToken = default)
    {
        var frameCount = 0;

        using var decoder = stream.CodecParameters.CreateDecoder();
        decoder.StandardCompliance = StandardCompliance.Experimental;
        decoder.Open();

        while (!cancellationToken.IsCancellationRequested)
        {
            AVFrame? frame;
            try
            {
                // try to get a decoded frame from AVCodecContext
                frame = decoder.ReceiveFrame();
            }
            catch (EndOfStreamException)
            {
                // this AVCodecContext has been flushed and no more frames are available
                break;
            }

            if (frame is { })
            {
                await frames.WriteAsync(frame, cancellationToken);
                ++frameCount;

                // continue to read next packet 
                continue;
            }

            if (!await packets.WaitToReadAsync(cancellationToken))
            {
                // flush AVCodecContext when no more packets are available
                decoder.SendPacket(null);
                break;
            }

            var packet = await packets.ReadAsync(cancellationToken);
            decoder.SendPacket(packet);
        }

        if (!cancellationToken.IsCancellationRequested)
            frames.Complete();
        else
            throw new OperationCanceledException(cancellationToken);

        return frameCount;
    }
}