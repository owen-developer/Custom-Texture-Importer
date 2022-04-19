using System.Runtime.InteropServices;

namespace Custom_Texture_Importer.Utils.Libs;

public class Oodle
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    public Oodle(string DllPath = null)
    {
        if (DllPath != null)
            SetDllDirectory(DllPath);
    }

    [DllImport("oo2core_5_win64.dll")]
    public static extern int OodleLZ_Compress(OodleFormat format, byte[]? decompressedBuffer, long decompressedSize,
            byte[] compressedBuffer, OodleCompressionLevel compressionLevel, uint a, uint b, uint c,
            ThreadModule threadModule); // Oodle dll method

    public uint GetCompressedBounds(uint BufferSize)
    {
        return BufferSize + 274 * ((BufferSize + 0x3FFFF) / 0x40000);
    }

    public static byte[] OodleCompress(byte[]? decompressedBuffer, int decompressedSize, OodleFormat format,
        OodleCompressionLevel compressionLevel, uint a)
    {
        var array = new byte[(uint)decompressedSize + 274U * (((uint)decompressedSize + 262143U) / 262144U)]; // Initializes array with compressed array size
        var compressedBytes = new byte[a + (uint)OodleLZ_Compress(format, decompressedBuffer, // Initializes the array we will be returning
            decompressedSize, array, compressionLevel, 0U, 0U,
            0U, 0U) - (int)a];
        Buffer.BlockCopy(array, 0, compressedBytes, 0, OodleLZ_Compress(format, decompressedBuffer, decompressedSize,
            array, compressionLevel, 0U, 0U,
            0U, 0U)); // Combines the two arrays
        return compressedBytes;
    }

    public byte[] Compress(byte[] decompressed)
    {
        uint @uint; // Needs to be outside so it always has a value
        try
        {
            @uint = (uint)OodleLZ_Compress(OodleFormat.Kraken, decompressed, // Get decompressed buffer
                decompressed.Length, // Get decompressed length
                new byte[(int)(uint)decompressed.Length + 274U *
                    (((uint)decompressed.Length + 262143U) / 262144U)], // Get compressed size
                OodleCompressionLevel.Optimal5, 0U, 0U, 0U, 0);
        }
        catch (AccessViolationException)
        {
            @uint = 64U; // Just in case there is protected memory
        }

        return OodleCompress(decompressed, decompressed.Length,
            OodleFormat.Kraken, OodleCompressionLevel.Optimal5, @uint); // Writing the data
    }
}

public enum ThreadModule : uint
{
}

public enum OodleFormat : uint
{
    LZH = 0,
    LZHLW = 1,
    LZNIB = 2,
    None = 3,
    LZB16 = 4,
    LZBLW = 5,
    LZA = 6,
    LZNA = 7,
    Kraken = 8,
    Mermaid = 9,
    BitKnit = 10,
    Selkie = 11,
    Hydra = 12,
    Leviathan = 13
}

public enum OodleCompressionLevel : ulong
{
    None = 0,
    SuperFast = 1,
    VeryFast = 2,
    Fast = 3,
    Normal = 4,
    Optimal1 = 5,
    Optimal2 = 6,
    Optimal3 = 7,
    Optimal4 = 8,
    Optimal5 = 9
}

public enum CompressionType : uint // Used for decompression so not needed here, unless someone wants to add it
{
    Unknown,
    Oodle,
    Zlib
}
