using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.PrimaryContent.Models;

public sealed class GlbMeshExporter : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is IMesh mesh && mesh.IsSet())
		{
			exportCollection = new GlbExportCollection(this, asset);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public bool Export(IUnityObjectBase asset, string path)
	{
		byte[] data = ExportBinary((IMesh)asset);
		if (data.Length == 0)
		{
			return false;
		}
		File.WriteAllBytes(path, data);
		return true;
	}

	private static byte[] ExportBinary(IMesh mesh)
	{
		SceneBuilder sceneBuilder = GlbMeshBuilder.Build(mesh);

		SharpGLTF.Schema2.WriteSettings writeSettings = new();

		return sceneBuilder.ToGltf2().WriteGLB(writeSettings).ToArray();
	}
}
