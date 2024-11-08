﻿using AssetRipper.Assets;
using AssetRipper.Export.Modules.Textures;

namespace AssetRipper.Export.UnityProjects.Textures;

public sealed class TextureArrayAssetExportCollection : AssetExportCollection<IUnityObjectBase>
{
	public TextureArrayAssetExportCollection(TextureArrayAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
	{
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		return "bytes";// ((TextureArrayAssetExporter)AssetExporter).ImageExportFormat.GetFileExtension();
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		return ImporterFactory.GenerateTextureImporter(container, Asset);
	}
}
