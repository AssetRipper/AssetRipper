using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasEditorData;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SpriteAtlasExtensions
	{
		public static void ConvertToEditorFormat(this ISpriteAtlas atlas)
		{
			ISpriteAtlasEditorData data = atlas.EditorData_C687078895;

			data.TextureSettings.Initialize();
			data.PackingParameters?.Initialize();
			data.PackingSettings?.Initialize();
			data.VariantMultiplier = 1;
			data.BindAsDefault = true;

			data.Packables.Clear();
			data.Packables.Capacity = atlas.PackedSprites_C687078895.Count;
			PPtrAccessList<PPtr_Object_5_0_0, Classes.ClassID_0.Object> packables = data.Packables.ToPPtrAccessList<PPtr_Object_5_0_0, Classes.ClassID_0.Object>(atlas.Collection);
			foreach (ISprite? sprite in atlas.PackedSprites_C687078895P)
			{
				packables.Add(sprite as Classes.ClassID_0.Object);
			}
		}
	}
}
