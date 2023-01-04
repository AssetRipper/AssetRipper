using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Collections;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;

namespace AssetRipper.Export.UnityProjects.Textures
{
	/// <summary>
	/// Exports sprites as YAML assets.
	/// </summary>
	public sealed class YamlSpriteExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset) => asset is ISprite or ISpriteAtlas;

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			if (asset is ISpriteAtlas)
			{
				return new EmptyExportCollection();
			}

			return new AssetExportCollection(this, asset);
		}
	}
}
