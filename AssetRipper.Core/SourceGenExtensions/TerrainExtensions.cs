using AssetRipper.Core.Classes.Renderer;
using AssetRipper.Core.Classes.Terrain;
using AssetRipper.SourceGenerated.Classes.ClassID_218;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TerrainExtensions
	{
		public static void ConvertToEditorFormat(this ITerrain terrain)
		{
			terrain.ScaleInLightmap_C218 = 0.0512f;
		}

		public static ShadowCastingMode GetShadowCastingMode(this ITerrain terrain)
		{
			if (terrain.Has_ShadowCastingMode_C218())
			{
				return (ShadowCastingMode)terrain.ShadowCastingMode_C218;
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
				return terrain.ShadowCastingMode_C218 != (int)ShadowCastingMode.Off;
			}
		}

		public static MaterialType GetMaterialType(this ITerrain terrain)
		{
			//if (ToSerializedVersion(version) > 2)
			//{
			//	return MaterialType;
			//}
			//return MaterialType == MaterialType.BuiltInStandard ? MaterialType.BuiltInLegacyDiffuse : MaterialType.Custom;
			return (MaterialType)terrain.MaterialType_C218;
		}

		public static ReflectionProbeUsage GetReflectionProbeUsage(this ITerrain terrain)
		{
			return (ReflectionProbeUsage)terrain.ReflectionProbeUsage_C218;
		}
	}
}
