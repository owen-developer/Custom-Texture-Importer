namespace Custom_Texture_Importer.Utils;

public class FileUtil
{
    //Originally: https://stackoverflow.com/a/332667/12897035
    public static int IndexOfSequence(byte[] buffer, byte[] pattern)
    {
        var i = Array.IndexOf(buffer, pattern[0], 0);
        while (i >= 0 && i <= buffer.Length - pattern.Length)
        {
            var segment = new byte[pattern.Length];
            Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
            if (segment.SequenceEqual(pattern))
                return i;
            i = Array.IndexOf(buffer, pattern[0], i + 1);
        }

        return -1;
    }
}