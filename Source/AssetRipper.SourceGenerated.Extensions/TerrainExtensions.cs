using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TerrainExtensions
{
	public static ShadowCastingMode GetShadowCastingMode(this ITerrain terrain)
	{
		if (terrain.Has_ShadowCastingMode())
		{
			return terrain.ShadowCastingModeE;
		}
		else
		{
			return terrain.CastShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
		}
	}

	public static bool GetCastShadows(this ITerrain terrain)
	{
		if (terrain.Has_CastShadows())
		{
			return terrain.CastShadows;
		}
		else
		{
			return terrain.ShadowCastingModeE != ShadowCastingMode.Off;
		}
	}
}
