using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;
using LibavSharp.Core.AVCodec;
using LibavSharp.Core.AVFormat;
using LibavSharp.Core.AVUtil;
using LibavSharp.Extensions.Demuxing;
using LibavSharp.Extensions.Muxing;
using LibavSharp.Extensions.Resampling;

if (args.Length != 2)
{
    Console.WriteLine($"Usage: {Path.GetFileName(Assembly.GetExecutingAssembly().Location)} <input> <output>");
    return;
}

// input format setup

using var inFormat = AVFormatContext.OpenInput(args[0]);
using var inAudioStream = inFormat.FindBestStream(AVMediaType.Audio);

// output codec setup

const AVCodecId audioCodecId = AVCodecId.Opus;
using var audioCodec = AVCodec.FindEncoder((int) audioCodecId);

using var audioCodecParams = AVCodecParameters.Create();
audioCodecParams.CodecId = (int) audioCodecId;
audioCodecParams.CodecType = AVMediaType.Audio;
audioCodecParams.BitRate = 96000;
audioCodecParams.Channels = 2; // stereo
audioCodecParams.ChannelLayout = (ulong) AVChannelLayout.GetDefaultChannelLayout(audioCodecParams.Channels);
audioCodecParams.SampleFormat = audioCodec.GetSupportedSampleFormats()[0];
audioCodecParams.SampleRate = 48000; // 48 kHz

using var audioEncoder = new AVCodecContext(AVCodec.FindEncoder((int) audioCodecId))
{
    StandardCompliance = StandardCompliance.Experimental
};
audioCodecParams.CopyToContext(audioEncoder);
audioEncoder.Open();

// output format setup

await using var outputStream = new MemoryStream();
using var outputIOContext = new StreamIOContext(outputStream, true);

using var outputFormat = AVOutputFormat.GuessFormat(filename: Path.GetFileName(args[1]));
using var outFormat = AVFormatContext.OpenOutput(outputIOContext, outputFormat);
outFormat.Url = Path.GetFileName(args[1]);

using var outStream = outFormat.NewStream(audioCodec);
outStream.CodecParameters.CopyFromContext(audioEncoder);

outFormat.WriteHeader();

// transcode

var inputPackets = Channel.CreateUnbounded<AVPacket>();
var inputFrames = Channel.CreateUnbounded<AVFrame>();
var outputFrames = Channel.CreateUnbounded<AVFrame>();
var outputPackets = Channel.CreateUnbounded<AVPacket>();

var inputStreamMapping = new Dictionary<int, ChannelWriter<AVPacket>>()
{
    {inAudioStream.Index, inputPackets.Writer}
};

FormatReader.DiscardUnusedStreams(inputStreamMapping.Keys, inFormat);

var stopwatch = new Stopwatch();
stopwatch.Start();

var transcode = Task.WhenAll(
    FormatReader.ReadPacketsAsync(inputStreamMapping, inFormat).ContinueWith(task =>
    {
        Console.WriteLine($"{task.Result} packets read from input file in {stopwatch.ElapsedMilliseconds} ms");
    }),
    StreamDecoder.DecodeFramesAsync(inputFrames.Writer, inputPackets.Reader, inAudioStream).ContinueWith(task =>
    {
        Console.WriteLine($"{task.Result} frames decoded from input stream in {stopwatch.ElapsedMilliseconds} ms");
    }),
    new ResampleFramer(outputFrames.Writer, inputFrames.Reader, audioEncoder).RunAsync().ContinueWith(task =>
    {
        Console.WriteLine($"{task.Result} frames resampled in {stopwatch.ElapsedMilliseconds} ms");
    }),
    StreamEncoder.EncodeFramesAsync(outputPackets.Writer, outputFrames.Reader, audioEncoder).ContinueWith(task =>
    {
        Console.WriteLine($"{task.Result} packets encoded in {stopwatch.ElapsedMilliseconds} ms");
    })
);

await transcode;

// write packets to output stream

var packetReader = outputPackets.Reader;
while (await packetReader.WaitToReadAsync())
{
    var packet = await packetReader.ReadAsync();
    outFormat.WriteFrameInterleaved(packet);
}

outFormat.WriteTrailer();

Console.WriteLine($"Output stream is {outputStream.Length} bytes long");

// copy to output file

await using var outputFile = File.OpenWrite(args[1]);
outputStream.Seek(0, SeekOrigin.Begin);
await outputStream.CopyToAsync(outputFile);

Console.WriteLine("Done writing output file to disk");