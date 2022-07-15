using AssetRipper.SourceGenerated.Classes.ClassID_687078895;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SpriteAtlasExtensions
	{
		public static void ConvertToEditorFormat(this ISpriteAtlas atlas)
		{
			atlas.EditorData_C687078895.ConvertToEditorFormat(atlas.PackedSprites_C687078895);
		}
	}
}
