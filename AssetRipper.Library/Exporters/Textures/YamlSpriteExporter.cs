using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Subclasses.SecondarySpriteTexture;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;
using System.Drawing;
using System.Numerics;

namespace AssetRipper.Library.Exporters.Textures
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
