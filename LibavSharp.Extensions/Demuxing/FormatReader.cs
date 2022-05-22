using System.Threading.Channels;
using LibavSharp.Core.AVCodec;
using LibavSharp.Core.AVFormat;

namespace LibavSharp.Extensions.Demuxing;

public static class FormatReader
{
    /// <param name="usedStreamIndices">stream indices NOT to be discarded</param>
    /// <param name="format">AVFormatContext to be operated on</param>
    public static void DiscardUnusedStreams(ICollection<int> usedStreamIndices, AVFormatContext format)
    {
        for (var i = 0; i < format.StreamCount; ++i)
        {
            if (usedStreamIndices.Contains(i)) continue;

            format.GetStream(i).Discard = AVDiscard.All;
        }
    }

    /// <param name="destinations">packet destinations of streams, indexed by stream index</param>
    /// <param name="format">AVFormatContext to be read</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>number of packets read</returns>
    public static async Task<int> ReadPacketsAsync(IReadOnlyDictionary<int, ChannelWriter<AVPacket>> destinations,
        AVFormatContext format, CancellationToken cancellationToken = default)
    {
        // await a immediate task to avoid caller being blocked by the first ReadFrame call
        await Task.Delay(TimeSpan.Zero, CancellationToken.None);

        var packetCount = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var packet = new AVPacket();
            try
            {
                try
                {
                    format.ReadFrame(packet);
                }
                catch (EndOfStreamException)
                {
                    foreach (var completedWriter in destinations.Values)
                    {
                        completedWriter.Complete();
                    }

                    break;
                }

                if (destinations.TryGetValue(packet.StreamIndex, out var writer))
                {
                    await writer.WriteAsync(packet, cancellationToken);
                    packet = null; // release ownership to next consumer
                }

                ++packetCount;
            }
            finally
            {
                packet?.Dispose(); // dispose unused packets (not written to channels)
            }
        }

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(cancellationToken);

        return packetCount;
    }
}