﻿using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;

namespace AssetRipper.Export.PrimaryContent.Models;

public sealed class GlbTerrainExporter : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is ITerrainData)
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
		SceneBuilder sceneBuilder = GlbTerrainBuilder.Build((ITerrainData)asset);
		using Stream fileStream = fileSystem.File.Create(path);
		sceneBuilder.ToGltf2().WriteGLB(fileStream, new WriteSettings() { MergeBuffers = false });
		return true;
	}
}
