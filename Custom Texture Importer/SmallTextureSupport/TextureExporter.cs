using System;
using System.IO;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Texture;

namespace Custom_Texture_Importer.SmallTextureSupport;

public class TextureExporter
{
    private readonly DefaultFileProvider _provider;
    private UTexture2D Texture;
    
    public TextureExporter(DefaultFileProvider provider)
    {
        _provider = provider;
    }
    
    public UTexture2D ExportFromPath(string gamePath) 
        =>  Texture = (UTexture2D)_provider.LoadObject(gamePath);
}