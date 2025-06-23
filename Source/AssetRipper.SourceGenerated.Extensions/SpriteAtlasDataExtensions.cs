using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SpriteAtlasDataExtensions
{
	public static bool IsPacked(this ISpriteAtlasData data) => (data.SettingsRaw & 1) != 0;
	public static SpritePackingMode GetPackingMode(this ISpriteAtlasData data) => (SpritePackingMode)(data.SettingsRaw >> 1 & 1);
	public static SpritePackingRotation GetPackingRotation(this ISpriteAtlasData data) => (SpritePackingRotation)(data.SettingsRaw >> 2 & 0xF);
	public static SpriteMeshType GetMeshType(this ISpriteAtlasData data) => (SpriteMeshType)(data.SettingsRaw >> 6 & 0x1);
}
