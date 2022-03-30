using Custom_Texture_Importer.Compression.Utils;
using System;

namespace Custom_Texture_Importer.Compression
{
    public static class Oodle
    {
        public static byte[] Compress(byte[] decompressed)
        {
            uint @uint; // Needs to be outside so it always has a value
            try
            {
                @uint = (uint) OodleStream.OodleLZ_Compress(OodleFormat.Kraken, decompressed, // Get decompressed buffer
                    decompressed.Length, // Get decompressed length
                    new byte[(int) (uint)decompressed.Length + 274U *
                        (((uint)decompressed.Length + 262143U) / 262144U)], // Get compressed size
                    OodleCompressionLevel.Optimal5, 0U, 0U, 0U, 0);
            }
            catch (AccessViolationException)
            {
                @uint = 64U; // Just in case there is protected memory
            }
            
            return OodleStream.OodleCompress(decompressed, decompressed.Length,
                OodleFormat.Kraken, OodleCompressionLevel.Optimal5, @uint); // Writing the data
        }
    }
}
