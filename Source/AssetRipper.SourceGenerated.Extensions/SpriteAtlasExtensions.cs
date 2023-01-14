using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Sprite;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasEditorData;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SpriteAtlasExtensions
	{
		public static void ConvertToEditorFormat(this ISpriteAtlas atlas)
		{
			atlas.EditorData_C687078895.ConvertToEditorFormat(atlas.PackedSprites_C687078895);
		}

		private static void ConvertToEditorFormat(this ISpriteAtlasEditorData data, IReadOnlyList<PPtr_Sprite_5_0_0> packedSprites)
		{
			data.TextureSettings.Initialize();
			data.PackingParameters?.Initialize();
			data.PackingSettings?.Initialize();
			data.VariantMultiplier = 1;
			data.BindAsDefault = true;

			data.Packables.Clear();
			data.Packables.Capacity = packedSprites.Count;
			foreach (PPtr_Sprite_5_0_0 sprite in packedSprites)
			{
				data.Packables.AddNew().CopyValues((PPtr<ISprite>)sprite);
			}
		}
	}
}
