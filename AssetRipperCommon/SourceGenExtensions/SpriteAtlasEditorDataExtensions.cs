using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Sprite_;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasEditorData;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SpriteAtlasEditorDataExtensions
	{
		public static void ConvertToEditorFormat(this ISpriteAtlasEditorData data, IReadOnlyList<PPtr_Sprite__5_0_0_f4> packedSprites)
		{
			data.TextureSettings.Initialize();
			data.PackingParameters?.Initialize();
			data.PackingSettings?.Initialize();
			data.VariantMultiplier = 1;
			data.BindAsDefault = true;

			data.Packables.Clear();
			data.Packables.Capacity = packedSprites.Count;
			foreach (PPtr_Sprite__5_0_0_f4 sprite in packedSprites)
			{
				data.Packables.AddNew().CopyValues(sprite);
			}
		}
	}
}
