using System.Runtime.InteropServices;

namespace Custom_Texture_Importer.Compression.Utils
{
    public class OodleStream
    {
        [DllImport("oo2core_5_win64.dll")]
        public static extern int OodleLZ_Compress(OodleFormat format, byte[]? decompressedBuffer, long decompressedSize,
            byte[] compressedBuffer, OodleCompressionLevel compressionLevel, uint a, uint b, uint c,
            ThreadModule threadModule); // Oodle dll method

        public static byte[] OodleCompress(byte[]? decompressedBuffer, int decompressedSize, OodleFormat format,
            OodleCompressionLevel compressionLevel, uint a)
        {
            var array = new byte[(uint) decompressedSize + 274U * (((uint) decompressedSize + 262143U) / 262144U)]; // Initializes array with compressed array size
            var compressedBytes = new byte[a + (uint) OodleLZ_Compress(format, decompressedBuffer, // Initializes the array we will be returning
                decompressedSize, array, compressionLevel, 0U, 0U,
                0U, 0U) - (int) a];
            Buffer.BlockCopy(array, 0, compressedBytes, 0, OodleLZ_Compress(format, decompressedBuffer, decompressedSize,
                array, compressionLevel, 0U, 0U,
                0U, 0U)); // Combines the two arrays
            return compressedBytes;
        }
    }
}