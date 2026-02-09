using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;

namespace AssetRipper.Export.UnityProjects.Textures;

/// <summary>
/// Exports sprites as YAML assets.
/// </summary>
public sealed class YamlSpriteExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset switch
		{
			ISprite sprite => new AssetExportCollection<ISprite>(this, sprite),
			ISpriteAtlas => EmptyExportCollection.Instance,
			_ => null,
		};
		return exportCollection is not null;
	}
}
