using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class TerrainExtensions
	{
		public static ShadowCastingMode GetShadowCastingMode(this ITerrain terrain)
		{
			if (terrain.Has_ShadowCastingMode_C218())
			{
				return terrain.ShadowCastingMode_C218E;
			}
			else
			{
				return terrain.CastShadows_C218 ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
			}
		}

		public static bool GetCastShadows(this ITerrain terrain)
		{
			if (terrain.Has_CastShadows_C218())
			{
				return terrain.CastShadows_C218;
			}
			else
			{
				return terrain.ShadowCastingMode_C218E != ShadowCastingMode.Off;
			}
		}
	}
}
