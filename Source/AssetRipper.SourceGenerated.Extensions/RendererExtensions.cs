using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Enums;
using System.Diagnostics;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class RendererExtensions
	{
		public static string? FindMaterialPropertyNameByCRC28(this IRenderer renderer, uint crc)
		{
			foreach (IMaterial? material in renderer.Materials_C25P)
			{
				string? property = material?.FindPropertyNameByCRC28(crc);
				if (property is not null)
				{
					return property;
				}
			}
			return null;
		}

		public static ShadowCastingMode GetShadowCastingMode(this IRenderer renderer)
		{
			return renderer.Has_CastShadows_C25_Byte()
				? (ShadowCastingMode)renderer.CastShadows_C25_Byte
				: renderer.CastShadows_C25_Boolean
					? ShadowCastingMode.On
					: ShadowCastingMode.Off;
		}

		public static MotionVectorGenerationMode GetMotionVectors(this IRenderer renderer)
		{
			return (MotionVectorGenerationMode)renderer.MotionVectors_C25;
		}

		public static LightProbeUsage GetLightProbeUsage(this IRenderer renderer)
		{
			return renderer.Has_LightProbeUsage_C25()
				? renderer.LightProbeUsage_C25E
				: renderer.UseLightProbes_C25
					? LightProbeUsage.BlendProbes
					: LightProbeUsage.Off;
		}

		public static ReflectionProbeUsage GetReflectionProbeUsage(this IRenderer renderer)
		{
			return renderer.Has_ReflectionProbeUsage_C25_Int32()
				? renderer.ReflectionProbeUsage_C25_Int32E
				: renderer.ReflectionProbeUsage_C25_ByteE;
		}

		public static ushort GetLightmapIndex(this IRenderer renderer)
		{
			if (renderer.Has_LightmapIndex_C25_UInt16())
			{
				return renderer.LightmapIndex_C25_UInt16;
			}
			else
			{
				Debug.Assert(renderer.Has_LightmapIndex_C25_Byte());
				byte value = renderer.LightmapIndex_C25_Byte;
				return value == byte.MaxValue ? ushort.MaxValue : value;
			}
		}
	}
}
