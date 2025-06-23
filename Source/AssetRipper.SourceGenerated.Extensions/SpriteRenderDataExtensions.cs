using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SpriteRenderDataExtensions
{
	public static bool IsPacked(this ISpriteRenderData spriteRenderData) => (spriteRenderData.SettingsRaw & 1) != 0;

	public static SpritePackingMode GetPackingMode(this ISpriteRenderData spriteRenderData)
	{
		return (SpritePackingMode)(spriteRenderData.SettingsRaw >> 1 & 1);
	}

	public static SpritePackingRotation GetPackingRotation(this ISpriteRenderData spriteRenderData)
	{
		return (SpritePackingRotation)(spriteRenderData.SettingsRaw >> 2 & 0xF);
	}

	public static SpriteMeshType GetMeshType(this ISpriteRenderData spriteRenderData)
	{
		return (SpriteMeshType)(spriteRenderData.SettingsRaw >> 6 & 0x1);
	}
}
