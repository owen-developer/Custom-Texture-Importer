using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;

namespace Custom_Texture_Importer.SkeletalMesh;

public class SkeletalMeshExporter
{
    private readonly DefaultFileProvider _provider;
    private USkeletalMesh _skelMesh;
    
    public SkeletalMeshExporter(DefaultFileProvider provider)
    {
        _provider = provider;
    }

    public USkeletalMesh ExportFromPath(string gamePath)
        =>  _skelMesh = (USkeletalMesh)_provider.LoadObject(gamePath);
}