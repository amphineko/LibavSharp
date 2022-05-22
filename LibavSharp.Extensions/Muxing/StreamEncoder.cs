using System.Threading.Channels;
using LibavSharp.Core.AVCodec;
using LibavSharp.Core.AVUtil;

namespace LibavSharp.Extensions.Muxing;

public static class StreamEncoder
{
    public static async Task<int> EncodeFramesAsync(ChannelWriter<AVPacket> packets, ChannelReader<AVFrame> frames,
        AVCodecContext encoder, CancellationToken cancellationToken = default)
    {
        var packetCount = 0;

        while (await frames.WaitToReadAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            using var frame = await frames.ReadAsync(cancellationToken);
            encoder.SendFrame(frame);

            packetCount += await EmitPackets(packets, encoder, cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested) return packetCount;

        // flush encoder

        encoder.SendFrame(null);
        packetCount += await EmitPackets(packets, encoder, cancellationToken);

        // close channel writer

        if (!cancellationToken.IsCancellationRequested)
            packets.Complete();
        else
            throw new OperationCanceledException(cancellationToken);

        return packetCount;
    }

    private static async Task<int> EmitPackets(ChannelWriter<AVPacket> packets, AVCodecContext encoder,
        CancellationToken cancellationToken)
    {
        var packetCount = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            AVPacket? packet = null;
            try
            {
                try
                {
                    packet = encoder.ReceivePacket();
                }
                catch (EndOfStreamException)
                {
                    break;
                }

                if (packet is not { }) break;

                await packets.WriteAsync(packet, cancellationToken);
                packet = null;
                ++packetCount;
            }
            finally
            {
                packet?.Dispose();
            }
        }

        return packetCount;
    }
}