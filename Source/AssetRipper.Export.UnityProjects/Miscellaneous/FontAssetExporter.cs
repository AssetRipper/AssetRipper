using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_128;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class FontAssetExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
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

		public override bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			IFont font = assets.OfType<IFont>().Single();
			File.WriteAllBytes(path, font.FontData);
			return true;
		}
	}
}
