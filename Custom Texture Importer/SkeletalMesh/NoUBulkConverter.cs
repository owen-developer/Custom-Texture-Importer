using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;

namespace Custom_Texture_Importer.SkeletalMesh;

public class NoUBulkConverter
{
    private USkeletalMesh _customMesh;

    public NoUBulkConverter(byte[] uasset, byte[] uexp, DefaultFileProvider _provider)
    {
        var pkg = new Package("pkg", uasset, uexp, null, null, _provider);
        _customMesh = (USkeletalMesh) pkg.GetExport(0);
    }

    public byte[] GetData(byte[] original, USkeletalMesh mesh)
    {
        var stream = new MemoryStream(original);
        for (int i = 0; i < mesh.LODModels.Length; i++)
        {
            var myMesh = mesh.LODModels[i];
            var myCustomMesh = _customMesh.LODModels[i];
            
            stream.Position = myMesh.BulkDataPosition;
            stream.Write(myCustomMesh.BulkData.Data, 0, myCustomMesh.BulkData.Data.Length);

            return stream.ToArray();

        }
        
        return Array.Empty<byte>();
    }
}