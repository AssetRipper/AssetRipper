using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_0;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasEditorData;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SpriteAtlasExtensions
{
	public static void ConvertToEditorFormat(this ISpriteAtlas atlas)
	{
		ISpriteAtlasEditorData data = atlas.EditorData;

		data.TextureSettings.Initialize();
		data.PackingParameters?.Initialize();
		data.PackingSettings?.Initialize();
		data.VariantMultiplier = 1;
		data.BindAsDefault = true;

		data.Packables.Clear();
		data.Packables.Capacity = atlas.PackedSprites.Count;
		PPtrAccessList<PPtr_Object_5, IObject> packables = data.Packables.ToPPtrAccessList<PPtr_Object_5, IObject>(atlas.Collection);
		foreach (ISprite? sprite in atlas.PackedSpritesP)
		{
			packables.Add(sprite);
		}
	}
}
