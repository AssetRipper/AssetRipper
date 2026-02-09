using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.Import.Logging;
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

	public bool Export(IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		SceneBuilder sceneBuilder = GlbMeshBuilder.Build((IMesh)asset);
		using Stream fileStream = fileSystem.File.Create(path);
		if (GlbWriter.TryWrite(sceneBuilder, fileStream, out string? errorMessage))
		{
			return true;
		}
		else
		{
			Logger.Error(LogCategory.Export, errorMessage);
			return false;
		}
	}
}
