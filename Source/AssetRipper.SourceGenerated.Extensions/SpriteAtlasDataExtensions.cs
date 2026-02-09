using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SpriteAtlasDataExtensions
{
	extension(ISpriteAtlasData data)
	{
		public bool IsPacked => (data.SettingsRaw & 1) != 0;

		public SpritePackingMode PackingMode => (SpritePackingMode)(data.SettingsRaw >> 1 & 1);

		public SpritePackingRotation PackingRotation => (SpritePackingRotation)(data.SettingsRaw >> 2 & 0xF);

		public SpriteMeshType MeshType => (SpriteMeshType)(data.SettingsRaw >> 6 & 0x1);
	}
}
