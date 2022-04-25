using System;
using System.IO;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;

namespace Custom_Texture_Importer.SmallTextureSupport;

public class TextureExporter
{
    private DefaultFileProvider _provider;
    private UTexture2D Texture;
    private UTexture2D CustomTexture;
    
    public TextureExporter(DefaultFileProvider provider)
    {
        _provider = provider;
    }
    
    public UTexture2D ExportFromPath(string gamePath) 
        =>  Texture = (UTexture2D)_provider.LoadObject(gamePath);

    public void TryConvert(byte[] uasset, byte[] uexp)
    {
        var pkg = new Package("pkg", uasset, uexp, null, null, _provider);
        CustomTexture = (UTexture2D)pkg.GetExport(0);
    }

    public byte[] GetData(byte[] original)
    {
        var stream = new MemoryStream(original);
        for (int i = 0; i < Texture.Mips.Length; i++)
        {
            var myMip = Texture.Mips[i];
            var myCustomMip = CustomTexture.Mips[i];
            if (myMip.Data.Data.Length != myCustomMip.Data.Data.Length)
            {
                stream.Position = myMip.Position;
                stream.Write(myCustomMip.Data.Data, 0, myCustomMip.Data.Data.Length);

                stream.Position = myMip.SizeOffset;
                stream.Write(BitConverter.GetBytes(myCustomMip.SizeX), 0, 4);
                stream.Write(BitConverter.GetBytes(myCustomMip.SizeY), 0, 4);
            }
            else
            {
                stream.Position = myMip.Position;
                stream.Write(myCustomMip.Data.Data, 0, myCustomMip.Data.Data.Length);
            }
        }

        return stream.ToArray();
    }
}