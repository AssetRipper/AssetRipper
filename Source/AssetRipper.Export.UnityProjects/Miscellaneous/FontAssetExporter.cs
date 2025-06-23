using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_128;

namespace AssetRipper.Export.UnityProjects.Miscellaneous;

public sealed class FontAssetExporter : BinaryAssetExporter
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset.MainAsset is IFont font && IsValidData(font.FontData))
		{
			exportCollection = new FontAssetExportCollection(this, font);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public override bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, FileSystem fileSystem)
	{
		IFont font = assets.OfType<IFont>().Single();
		fileSystem.File.WriteAllBytes(path, font.FontData);
		return true;
	}
}
