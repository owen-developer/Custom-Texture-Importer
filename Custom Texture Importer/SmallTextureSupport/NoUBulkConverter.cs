using System.Runtime.CompilerServices;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Objects;
using Custom_Texture_Importer.Writers;

namespace Custom_Texture_Importer.SmallTextureSupport;

public class NoUBulkConverter
{
    private UTexture2D _customTexture;

    public NoUBulkConverter(byte[] uasset, byte[] uexp, DefaultFileProvider _provider)
    {
        var pkg = new Package("pkg", uasset, uexp, null, null, _provider);
        _customTexture = (UTexture2D)pkg.GetExport(0);
    }

    public unsafe byte[] GetData(byte[] original, UTexture2D texture)
    {
        var stream = new MemoryStream(original);
        long pos = texture.Mips[0].Position;
        for (int i = 0; i < texture.Mips.Length; i++)
        {
            var myMip = texture.Mips[i];
            var myCustomMip = _customTexture.Mips[i];

            var data = WriteBulkData(myCustomMip.Data);

            stream.Position = pos;
            stream.Write(data);
            pos += data.LongLength;
        }

        return stream.ToArray();
    }

    public static byte[] WriteBulkData(FByteBulkData data)
    {
        var mem = new Writer();
        
        // Header
        mem.Write(data.Header.BulkDataFlags);
        mem.Write(BitConverter.GetBytes(data.Header.ElementCount));
        mem.Write(BitConverter.GetBytes(data.Header.SizeOnDisk));
        mem.Write(BitConverter.GetBytes(data.Header.OffsetInFile));
        
        // Flags
        mem.Write(data.BulkDataFlags);
        
        // Data
        mem.WriteBytes(data.Data);

        return mem.Data;
    }
}