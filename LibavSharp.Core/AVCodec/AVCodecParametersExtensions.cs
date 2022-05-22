namespace LibavSharp.Core.AVCodec;

public static class AVCodecParametersExtensions
{
    public static AVCodecContext CreateDecoder(this AVCodecParameters parameters)
    {
        var codec = AVCodec.FindDecoder(parameters.CodecId);
        return CreateContext(parameters, codec);
    }

    public static AVCodecContext CreateEncoder(this AVCodecParameters parameters)
    {
        var codec = AVCodec.FindEncoder(parameters.CodecId);
        return CreateContext(parameters, codec);
    }

    private static AVCodecContext CreateContext(AVCodecParameters parameters, AVCodec codec)
    {
        var context = new AVCodecContext(codec);
        parameters.CopyToContext(context);
        return context;
    }
}