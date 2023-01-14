using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Enums;

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

		public static void ConvertToEditorFormat(this IRenderer renderer)
		{
			renderer.ScaleInLightmap_C25 = 1.0f;
			renderer.ReceiveGI_C25 = (int)ReceiveGI.Lightmaps;
			renderer.PreserveUVs_C25 = false;
			renderer.IgnoreNormalsForChartDetection_C25 = false;
			renderer.ImportantGI_C25 = false;
			renderer.StitchLightmapSeams_C25 = false;
			renderer.SelectedEditorRenderState_C25 = (int)(EditorSelectedRenderState)3;
			renderer.MinimumChartSize_C25 = 4;
			renderer.AutoUVMaxDistance_C25 = 0.5f;
			renderer.AutoUVMaxAngle_C25 = 89.0f;
			renderer.LightmapParameters_C25P = null;
		}
	}
}
