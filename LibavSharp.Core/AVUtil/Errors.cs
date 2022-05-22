namespace LibavSharp.Core.AVUtil;

public class Errors
{
    public static int Again = OperatingSystem.IsMacOS() ? -35 : -11;

    public const int EndOfFile = -541478725;

    public const int StreamNotFound = -1381258232;
}