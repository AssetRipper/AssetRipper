using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Renderer;
using AssetRipper.SourceGenerated.Classes.ClassID_120;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_161;
using AssetRipper.SourceGenerated.Classes.ClassID_1971053207;
using AssetRipper.SourceGenerated.Classes.ClassID_199;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_212;
using AssetRipper.SourceGenerated.Classes.ClassID_227;
using AssetRipper.SourceGenerated.Classes.ClassID_23;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_26;
using AssetRipper.SourceGenerated.Classes.ClassID_331;
using AssetRipper.SourceGenerated.Classes.ClassID_483693784;
using AssetRipper.SourceGenerated.Classes.ClassID_73398921;
using AssetRipper.SourceGenerated.Classes.ClassID_96;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Material_;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class RendererExtensions
	{
		public static string? FindMaterialPropertyNameByCRC28(this IRenderer renderer, uint crc)
		{
			foreach (IPPtr_Material_ materialPtr in renderer.GetMaterials())
			{
				IMaterial? material = materialPtr.TryGetAsset(renderer.SerializedFile);
				if (material == null)
				{
					continue;
				}
				string? property = material.FindPropertyNameByCRC28(crc);
				if (property == null)
				{
					continue;
				}

				return property;
			}
			return null;
		}

		public static IEnumerable<IPPtr_Material_> GetMaterials(this IRenderer renderer)
		{
			return renderer switch
			{
				IMeshRenderer meshRenderer => meshRenderer.Materials_C23,
				IParticleRenderer particleRenderer => particleRenderer.Materials_C26,
				ITrailRenderer trailRenderer => trailRenderer.Materials_C96,
				ILineRenderer lineRenderer => lineRenderer.Materials_C120,
				ISkinnedMeshRenderer skinnedMeshRenderer => skinnedMeshRenderer.Materials_C137,
				IClothRenderer clothRenderer => clothRenderer.Materials_C161,
				IParticleSystemRenderer particleSystemRenderer => particleSystemRenderer.Materials_C199,
				ISpriteRenderer spriteRenderer => spriteRenderer.Materials_C212,
				IBillboardRenderer billboardRenderer => billboardRenderer.Materials_C227,
				ISpriteMask spriteMask => spriteMask.Materials_C331,
				ITilemapRenderer tilemapRenderer => tilemapRenderer.Materials_C483693784,
				ISpriteShapeRenderer spriteShapeRenderer => spriteShapeRenderer.Materials_C1971053207,
				IVFXRenderer vfxRenderer => vfxRenderer.Materials_C73398921 ?? Enumerable.Empty<IPPtr_Material_>(),
				_ => throw new NotSupportedException(renderer.GetType().FullName)
			};
		}

		public static ShadowCastingMode GetShadowCastingMode(this IRenderer renderer)
		{
			return renderer switch
			{
				IMeshRenderer meshRenderer => meshRenderer.Has_CastShadows_C23_Byte()
					? (ShadowCastingMode)meshRenderer.CastShadows_C23_Byte
					: meshRenderer.CastShadows_C23_Boolean
						? ShadowCastingMode.On
						: ShadowCastingMode.Off,
				IParticleRenderer particleRenderer => particleRenderer.Has_CastShadows_C26_Byte()
					? (ShadowCastingMode)particleRenderer.CastShadows_C26_Byte
					: particleRenderer.CastShadows_C26_Boolean
						? ShadowCastingMode.On
						: ShadowCastingMode.Off,
				ITrailRenderer trailRenderer => trailRenderer.Has_CastShadows_C96_Byte()
					? (ShadowCastingMode)trailRenderer.CastShadows_C96_Byte
					: trailRenderer.CastShadows_C96_Boolean
						? ShadowCastingMode.On
						: ShadowCastingMode.Off,
				ILineRenderer lineRenderer => lineRenderer.Has_CastShadows_C120_Byte()
					? (ShadowCastingMode)lineRenderer.CastShadows_C120_Byte
					: lineRenderer.CastShadows_C120_Boolean
						? ShadowCastingMode.On
						: ShadowCastingMode.Off,
				ISkinnedMeshRenderer skinnedMeshRenderer => skinnedMeshRenderer.Has_CastShadows_C137_Byte()
					? (ShadowCastingMode)skinnedMeshRenderer.CastShadows_C137_Byte
					: skinnedMeshRenderer.CastShadows_C137_Boolean
						? ShadowCastingMode.On
						: ShadowCastingMode.Off,
				IClothRenderer clothRenderer => clothRenderer.CastShadows_C161
					? ShadowCastingMode.On
					: ShadowCastingMode.Off,
				IParticleSystemRenderer particleSystemRenderer => particleSystemRenderer.Has_CastShadows_C199_Byte()
					? (ShadowCastingMode)particleSystemRenderer.CastShadows_C199_Byte
					: particleSystemRenderer.CastShadows_C199_Boolean
						? ShadowCastingMode.On
						: ShadowCastingMode.Off,
				ISpriteRenderer spriteRenderer => spriteRenderer.Has_CastShadows_C212_Byte()
					? (ShadowCastingMode)spriteRenderer.CastShadows_C212_Byte
					: spriteRenderer.CastShadows_C212_Boolean
						? ShadowCastingMode.On
						: ShadowCastingMode.Off,
				IBillboardRenderer billboardRenderer => (ShadowCastingMode)billboardRenderer.CastShadows_C227,
				ISpriteMask spriteMask => (ShadowCastingMode)spriteMask.CastShadows_C331,
				ITilemapRenderer tilemapRenderer => (ShadowCastingMode)tilemapRenderer.CastShadows_C483693784,
				ISpriteShapeRenderer spriteShapeRenderer => (ShadowCastingMode)spriteShapeRenderer.CastShadows_C1971053207,
				IVFXRenderer vfxRenderer => (ShadowCastingMode)vfxRenderer.CastShadows_C73398921,
				_ => throw new NotSupportedException(renderer.GetType().FullName)
			};
		}

		public static MotionVectorGenerationMode GetMotionVectors(this IRenderer renderer)
		{
			return renderer switch
			{
				IMeshRenderer meshRenderer => (MotionVectorGenerationMode)meshRenderer.MotionVectors_C23,
				IParticleRenderer particleRenderer => (MotionVectorGenerationMode)particleRenderer.MotionVectors_C26,
				ITrailRenderer trailRenderer => (MotionVectorGenerationMode)trailRenderer.MotionVectors_C96,
				ILineRenderer lineRenderer => (MotionVectorGenerationMode)lineRenderer.MotionVectors_C120,
				ISkinnedMeshRenderer skinnedMeshRenderer => (MotionVectorGenerationMode)skinnedMeshRenderer.MotionVectors_C137,
				IClothRenderer => default,
				IParticleSystemRenderer particleSystemRenderer => (MotionVectorGenerationMode)particleSystemRenderer.MotionVectors_C199,
				ISpriteRenderer spriteRenderer => (MotionVectorGenerationMode)spriteRenderer.MotionVectors_C212,
				IBillboardRenderer billboardRenderer => (MotionVectorGenerationMode)billboardRenderer.MotionVectors_C227,
				ISpriteMask spriteMask => (MotionVectorGenerationMode)spriteMask.MotionVectors_C331,
				ITilemapRenderer tilemapRenderer => (MotionVectorGenerationMode)tilemapRenderer.MotionVectors_C483693784,
				ISpriteShapeRenderer spriteShapeRenderer => (MotionVectorGenerationMode)spriteShapeRenderer.MotionVectors_C1971053207,
				IVFXRenderer vfxRenderer => (MotionVectorGenerationMode)vfxRenderer.MotionVectors_C73398921,
				_ => throw new NotSupportedException(renderer.GetType().FullName)
			};
		}

		public static LightProbeUsage GetLightProbeUsage(this IRenderer renderer)
		{
			return renderer switch
			{
				IMeshRenderer meshRenderer => meshRenderer.Has_LightProbeUsage_C23()
					? (LightProbeUsage)meshRenderer.LightProbeUsage_C23
					: meshRenderer.UseLightProbes_C23
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				IParticleRenderer particleRenderer => particleRenderer.Has_LightProbeUsage_C26()
					? (LightProbeUsage)particleRenderer.LightProbeUsage_C26
					: particleRenderer.UseLightProbes_C26
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				ITrailRenderer trailRenderer => trailRenderer.Has_LightProbeUsage_C96()
					? (LightProbeUsage)trailRenderer.LightProbeUsage_C96
					: trailRenderer.UseLightProbes_C96
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				ILineRenderer lineRenderer => lineRenderer.Has_LightProbeUsage_C120()
					? (LightProbeUsage)lineRenderer.LightProbeUsage_C120
					: lineRenderer.UseLightProbes_C120
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				ISkinnedMeshRenderer skinnedMeshRenderer => skinnedMeshRenderer.Has_LightProbeUsage_C137()
					? (LightProbeUsage)skinnedMeshRenderer.LightProbeUsage_C137
					: skinnedMeshRenderer.UseLightProbes_C137
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				IClothRenderer => default,
				IParticleSystemRenderer particleSystemRenderer => particleSystemRenderer.Has_LightProbeUsage_C199()
					? (LightProbeUsage)particleSystemRenderer.LightProbeUsage_C199
					: particleSystemRenderer.UseLightProbes_C199
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				ISpriteRenderer spriteRenderer => spriteRenderer.Has_LightProbeUsage_C212()
					? (LightProbeUsage)spriteRenderer.LightProbeUsage_C212
					: spriteRenderer.UseLightProbes_C212
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				IBillboardRenderer billboardRenderer => billboardRenderer.Has_LightProbeUsage_C227()
					? (LightProbeUsage)billboardRenderer.LightProbeUsage_C227
					: billboardRenderer.UseLightProbes_C227
						? LightProbeUsage.BlendProbes
						: LightProbeUsage.Off,
				ISpriteMask spriteMask => (LightProbeUsage)spriteMask.LightProbeUsage_C331,
				ITilemapRenderer tilemapRenderer => (LightProbeUsage)tilemapRenderer.LightProbeUsage_C483693784,
				ISpriteShapeRenderer spriteShapeRenderer => (LightProbeUsage)spriteShapeRenderer.LightProbeUsage_C1971053207,
				IVFXRenderer vfxRenderer => (LightProbeUsage)vfxRenderer.LightProbeUsage_C73398921,
				_ => throw new NotSupportedException(renderer.GetType().FullName)
			};
		}

		public static ReflectionProbeUsage GetReflectionProbeUsage(this IRenderer renderer)
		{
			return renderer switch
			{
				IMeshRenderer meshRenderer => meshRenderer.Has_ReflectionProbeUsage_C23_Int32()
					? (ReflectionProbeUsage)meshRenderer.ReflectionProbeUsage_C23_Int32
					: (ReflectionProbeUsage)meshRenderer.ReflectionProbeUsage_C23_Byte,
				IParticleRenderer particleRenderer => particleRenderer.Has_ReflectionProbeUsage_C26_Int32()
					? (ReflectionProbeUsage)particleRenderer.ReflectionProbeUsage_C26_Int32
					: (ReflectionProbeUsage)particleRenderer.ReflectionProbeUsage_C26_Byte,
				ITrailRenderer trailRenderer => trailRenderer.Has_ReflectionProbeUsage_C96_Int32()
					? (ReflectionProbeUsage)trailRenderer.ReflectionProbeUsage_C96_Int32
					: (ReflectionProbeUsage)trailRenderer.ReflectionProbeUsage_C96_Byte,
				ILineRenderer lineRenderer => lineRenderer.Has_ReflectionProbeUsage_C120_Int32()
					? (ReflectionProbeUsage)lineRenderer.ReflectionProbeUsage_C120_Int32
					: (ReflectionProbeUsage)lineRenderer.ReflectionProbeUsage_C120_Byte,
				ISkinnedMeshRenderer skinnedMeshRenderer => skinnedMeshRenderer.Has_ReflectionProbeUsage_C137_Int32()
					? (ReflectionProbeUsage)skinnedMeshRenderer.ReflectionProbeUsage_C137_Int32
					: (ReflectionProbeUsage)skinnedMeshRenderer.ReflectionProbeUsage_C137_Byte,
				IClothRenderer => default,
				IParticleSystemRenderer particleSystemRenderer => particleSystemRenderer.Has_ReflectionProbeUsage_C199_Int32()
					? (ReflectionProbeUsage)particleSystemRenderer.ReflectionProbeUsage_C199_Int32
					: (ReflectionProbeUsage)particleSystemRenderer.ReflectionProbeUsage_C199_Byte,
				ISpriteRenderer spriteRenderer => spriteRenderer.Has_ReflectionProbeUsage_C212_Int32()
					? (ReflectionProbeUsage)spriteRenderer.ReflectionProbeUsage_C212_Int32
					: (ReflectionProbeUsage)spriteRenderer.ReflectionProbeUsage_C212_Byte,
				IBillboardRenderer billboardRenderer => billboardRenderer.Has_ReflectionProbeUsage_C227_Int32()
					? (ReflectionProbeUsage)billboardRenderer.ReflectionProbeUsage_C227_Int32
					: (ReflectionProbeUsage)billboardRenderer.ReflectionProbeUsage_C227_Byte,
				ISpriteMask spriteMask => (ReflectionProbeUsage)spriteMask.ReflectionProbeUsage_C331,
				ITilemapRenderer tilemapRenderer => (ReflectionProbeUsage)tilemapRenderer.ReflectionProbeUsage_C483693784,
				ISpriteShapeRenderer spriteShapeRenderer => (ReflectionProbeUsage)spriteShapeRenderer.ReflectionProbeUsage_C1971053207,
				IVFXRenderer vfxRenderer => (ReflectionProbeUsage)vfxRenderer.ReflectionProbeUsage_C73398921,
				_ => throw new NotSupportedException(renderer.GetType().FullName)
			};
		}

		public static RayTracingMode GetRayTracingMode(this IRenderer renderer)
		{
			return renderer switch
			{
				IMeshRenderer meshRenderer => (RayTracingMode)meshRenderer.RayTracingMode_C23,
				IParticleRenderer particleRenderer => default,
				ITrailRenderer trailRenderer => (RayTracingMode)trailRenderer.RayTracingMode_C96,
				ILineRenderer lineRenderer => (RayTracingMode)lineRenderer.RayTracingMode_C120,
				ISkinnedMeshRenderer skinnedMeshRenderer => (RayTracingMode)skinnedMeshRenderer.RayTracingMode_C137,
				IClothRenderer => default,
				IParticleSystemRenderer particleSystemRenderer => (RayTracingMode)particleSystemRenderer.RayTracingMode_C199,
				ISpriteRenderer spriteRenderer => (RayTracingMode)spriteRenderer.RayTracingMode_C212,
				IBillboardRenderer billboardRenderer => (RayTracingMode)billboardRenderer.RayTracingMode_C227,
				ISpriteMask spriteMask => (RayTracingMode)spriteMask.RayTracingMode_C331,
				ITilemapRenderer tilemapRenderer => (RayTracingMode)tilemapRenderer.RayTracingMode_C483693784,
				ISpriteShapeRenderer spriteShapeRenderer => (RayTracingMode)spriteShapeRenderer.RayTracingMode_C1971053207,
				IVFXRenderer vfxRenderer => (RayTracingMode)vfxRenderer.RayTracingMode_C73398921,
				_ => throw new NotSupportedException(renderer.GetType().FullName)
			};
		}

		public static void ConvertToEditorFormat(this IRenderer renderer)
		{
			switch (renderer)
			{
				case IMeshRenderer meshRenderer:
					ConvertToEditorFormat(meshRenderer);
					break;
				case IParticleRenderer particleRenderer:
					ConvertToEditorFormat(particleRenderer);
					break;
				case ITrailRenderer trailRenderer:
					ConvertToEditorFormat(trailRenderer);
					break;
				case ILineRenderer lineRenderer:
					ConvertToEditorFormat(lineRenderer);
					break;
				case ISkinnedMeshRenderer skinnedMeshRenderer:
					ConvertToEditorFormat(skinnedMeshRenderer);
					break;
				case IClothRenderer clothRenderer:
					ConvertToEditorFormat(clothRenderer);
					break;
				case IParticleSystemRenderer particleSystemRenderer:
					ConvertToEditorFormat(particleSystemRenderer);
					break;
				case ISpriteRenderer spriteRenderer:
					ConvertToEditorFormat(spriteRenderer);
					break;
				case IBillboardRenderer billboardRenderer:
					ConvertToEditorFormat(billboardRenderer);
					break;
				case ISpriteMask spriteMask:
					ConvertToEditorFormat(spriteMask);
					break;
				case ITilemapRenderer tilemapRenderer:
					ConvertToEditorFormat(tilemapRenderer);
					break;
				case ISpriteShapeRenderer spriteShapeRenderer:
					ConvertToEditorFormat(spriteShapeRenderer);
					break;
				case IVFXRenderer vfxRenderer:
					ConvertToEditorFormat(vfxRenderer);
					break;
				default:
					throw new NotSupportedException(renderer.GetType().FullName);
			}
		}

		private static void ConvertToEditorFormat(IVFXRenderer vfxRenderer)
		{
			vfxRenderer.ScaleInLightmap_C73398921 = 1.0f;
			vfxRenderer.ReceiveGI_C73398921 = (int)ReceiveGI.Lightmaps;
			vfxRenderer.PreserveUVs_C73398921 = false;
			vfxRenderer.IgnoreNormalsForChartDetection_C73398921 = false;
			vfxRenderer.ImportantGI_C73398921 = false;
			vfxRenderer.StitchLightmapSeams_C73398921 = false;
			vfxRenderer.SelectedEditorRenderState_C73398921 = (int)(EditorSelectedRenderState)3;
			vfxRenderer.MinimumChartSize_C73398921 = 4;
			vfxRenderer.AutoUVMaxDistance_C73398921 = 0.5f;
			vfxRenderer.AutoUVMaxAngle_C73398921 = 89.0f;
			vfxRenderer.LightmapParameters_C73398921?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(ISpriteShapeRenderer spriteShapeRenderer)
		{
			spriteShapeRenderer.ScaleInLightmap_C1971053207 = 1.0f;
			spriteShapeRenderer.ReceiveGI_C1971053207 = (int)ReceiveGI.Lightmaps;
			spriteShapeRenderer.PreserveUVs_C1971053207 = false;
			spriteShapeRenderer.IgnoreNormalsForChartDetection_C1971053207 = false;
			spriteShapeRenderer.ImportantGI_C1971053207 = false;
			spriteShapeRenderer.StitchLightmapSeams_C1971053207 = false;
			spriteShapeRenderer.SelectedEditorRenderState_C1971053207 = (int)(EditorSelectedRenderState)3;
			spriteShapeRenderer.MinimumChartSize_C1971053207 = 4;
			spriteShapeRenderer.AutoUVMaxDistance_C1971053207 = 0.5f;
			spriteShapeRenderer.AutoUVMaxAngle_C1971053207 = 89.0f;
			spriteShapeRenderer.LightmapParameters_C1971053207?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(ITilemapRenderer tilemapRenderer)
		{
			tilemapRenderer.ScaleInLightmap_C483693784 = 1.0f;
			tilemapRenderer.ReceiveGI_C483693784 = (int)ReceiveGI.Lightmaps;
			tilemapRenderer.PreserveUVs_C483693784 = false;
			tilemapRenderer.IgnoreNormalsForChartDetection_C483693784 = false;
			tilemapRenderer.ImportantGI_C483693784 = false;
			tilemapRenderer.StitchLightmapSeams_C483693784 = false;
			tilemapRenderer.SelectedEditorRenderState_C483693784 = (int)(EditorSelectedRenderState)3;
			tilemapRenderer.MinimumChartSize_C483693784 = 4;
			tilemapRenderer.AutoUVMaxDistance_C483693784 = 0.5f;
			tilemapRenderer.AutoUVMaxAngle_C483693784 = 89.0f;
			tilemapRenderer.LightmapParameters_C483693784?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(ISpriteMask spriteMask)
		{
			spriteMask.ScaleInLightmap_C331 = 1.0f;
			spriteMask.ReceiveGI_C331 = (int)ReceiveGI.Lightmaps;
			spriteMask.PreserveUVs_C331 = false;
			spriteMask.IgnoreNormalsForChartDetection_C331 = false;
			spriteMask.ImportantGI_C331 = false;
			spriteMask.StitchLightmapSeams_C331 = false;
			spriteMask.SelectedEditorRenderState_C331 = (int)(EditorSelectedRenderState)3;
			spriteMask.MinimumChartSize_C331 = 4;
			spriteMask.AutoUVMaxDistance_C331 = 0.5f;
			spriteMask.AutoUVMaxAngle_C331 = 89.0f;
			spriteMask.LightmapParameters_C331?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(IBillboardRenderer billboardRenderer)
		{
			billboardRenderer.ScaleInLightmap_C227 = 1.0f;
			billboardRenderer.ReceiveGI_C227 = (int)ReceiveGI.Lightmaps;
			billboardRenderer.PreserveUVs_C227 = false;
			billboardRenderer.IgnoreNormalsForChartDetection_C227 = false;
			billboardRenderer.ImportantGI_C227 = false;
			billboardRenderer.StitchLightmapSeams_C227 = false;
			billboardRenderer.SelectedEditorRenderState_C227 = (int)(EditorSelectedRenderState)3;
			billboardRenderer.MinimumChartSize_C227 = 4;
			billboardRenderer.AutoUVMaxDistance_C227 = 0.5f;
			billboardRenderer.AutoUVMaxAngle_C227 = 89.0f;
			billboardRenderer.LightmapParameters_C227?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(ISpriteRenderer spriteRenderer)
		{
			spriteRenderer.ScaleInLightmap_C212 = 1.0f;
			spriteRenderer.ReceiveGI_C212 = (int)ReceiveGI.Lightmaps;
			spriteRenderer.PreserveUVs_C212 = false;
			spriteRenderer.IgnoreNormalsForChartDetection_C212 = false;
			spriteRenderer.ImportantGI_C212 = false;
			spriteRenderer.StitchLightmapSeams_C212 = false;
			spriteRenderer.SelectedEditorRenderState_C212 = (int)(EditorSelectedRenderState)3;
			spriteRenderer.MinimumChartSize_C212 = 4;
			spriteRenderer.AutoUVMaxDistance_C212 = 0.5f;
			spriteRenderer.AutoUVMaxAngle_C212 = 89.0f;
			spriteRenderer.LightmapParameters_C212?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(IParticleSystemRenderer particleSystemRenderer)
		{
			particleSystemRenderer.ScaleInLightmap_C199 = 1.0f;
			particleSystemRenderer.ReceiveGI_C199 = (int)ReceiveGI.Lightmaps;
			particleSystemRenderer.PreserveUVs_C199 = false;
			particleSystemRenderer.IgnoreNormalsForChartDetection_C199 = false;
			particleSystemRenderer.ImportantGI_C199 = false;
			particleSystemRenderer.StitchLightmapSeams_C199 = false;
			particleSystemRenderer.SelectedEditorRenderState_C199 = (int)(EditorSelectedRenderState)3;
			particleSystemRenderer.MinimumChartSize_C199 = 4;
			particleSystemRenderer.AutoUVMaxDistance_C199 = 0.5f;
			particleSystemRenderer.AutoUVMaxAngle_C199 = 89.0f;
			particleSystemRenderer.LightmapParameters_C199?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(IClothRenderer clothRenderer)
		{
			clothRenderer.ScaleInLightmap_C161 = 1.0f;
			//others absent because cloth renderer not used in Unity 5+
		}

		private static void ConvertToEditorFormat(ISkinnedMeshRenderer skinnedMeshRenderer)
		{
			skinnedMeshRenderer.ScaleInLightmap_C137 = 1.0f;
			skinnedMeshRenderer.ReceiveGI_C137 = (int)ReceiveGI.Lightmaps;
			skinnedMeshRenderer.PreserveUVs_C137 = false;
			skinnedMeshRenderer.IgnoreNormalsForChartDetection_C137 = false;
			skinnedMeshRenderer.ImportantGI_C137 = false;
			skinnedMeshRenderer.StitchLightmapSeams_C137 = false;
			skinnedMeshRenderer.SelectedEditorRenderState_C137 = (int)(EditorSelectedRenderState)3;
			skinnedMeshRenderer.MinimumChartSize_C137 = 4;
			skinnedMeshRenderer.AutoUVMaxDistance_C137 = 0.5f;
			skinnedMeshRenderer.AutoUVMaxAngle_C137 = 89.0f;
			skinnedMeshRenderer.LightmapParameters_C137?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(ILineRenderer lineRenderer)
		{
			lineRenderer.ScaleInLightmap_C120 = 1.0f;
			lineRenderer.ReceiveGI_C120 = (int)ReceiveGI.Lightmaps;
			lineRenderer.PreserveUVs_C120 = false;
			lineRenderer.IgnoreNormalsForChartDetection_C120 = false;
			lineRenderer.ImportantGI_C120 = false;
			lineRenderer.StitchLightmapSeams_C120 = false;
			lineRenderer.SelectedEditorRenderState_C120 = (int)(EditorSelectedRenderState)3;
			lineRenderer.MinimumChartSize_C120 = 4;
			lineRenderer.AutoUVMaxDistance_C120 = 0.5f;
			lineRenderer.AutoUVMaxAngle_C120 = 89.0f;
			lineRenderer.LightmapParameters_C120?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(ITrailRenderer trailRenderer)
		{
			trailRenderer.ScaleInLightmap_C96 = 1.0f;
			trailRenderer.ReceiveGI_C96 = (int)ReceiveGI.Lightmaps;
			trailRenderer.PreserveUVs_C96 = false;
			trailRenderer.IgnoreNormalsForChartDetection_C96 = false;
			trailRenderer.ImportantGI_C96 = false;
			trailRenderer.StitchLightmapSeams_C96 = false;
			trailRenderer.SelectedEditorRenderState_C96 = (int)(EditorSelectedRenderState)3;
			trailRenderer.MinimumChartSize_C96 = 4;
			trailRenderer.AutoUVMaxDistance_C96 = 0.5f;
			trailRenderer.AutoUVMaxAngle_C96 = 89.0f;
			trailRenderer.LightmapParameters_C96?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(IParticleRenderer particleRenderer)
		{
			particleRenderer.ScaleInLightmap_C26 = 1.0f;
			//ReceiveGI only present in Unity 2019+
			particleRenderer.PreserveUVs_C26 = false;
			particleRenderer.IgnoreNormalsForChartDetection_C26 = false;
			particleRenderer.ImportantGI_C26 = false;
			particleRenderer.StitchLightmapSeams_C26 = false;
			particleRenderer.SelectedEditorRenderState_C26 = (int)(EditorSelectedRenderState)3;
			particleRenderer.MinimumChartSize_C26 = 4;
			particleRenderer.AutoUVMaxDistance_C26 = 0.5f;
			particleRenderer.AutoUVMaxAngle_C26 = 89.0f;
			particleRenderer.LightmapParameters_C26?.SetValues(0, 0);
		}

		private static void ConvertToEditorFormat(IMeshRenderer meshRenderer)
		{
			meshRenderer.ScaleInLightmap_C23 = 1.0f;
			meshRenderer.ReceiveGI_C23 = (int)ReceiveGI.Lightmaps;
			meshRenderer.PreserveUVs_C23 = false;
			meshRenderer.IgnoreNormalsForChartDetection_C23 = false;
			meshRenderer.ImportantGI_C23 = false;
			meshRenderer.StitchLightmapSeams_C23 = false;
			meshRenderer.SelectedEditorRenderState_C23 = (int)(EditorSelectedRenderState)3;
			meshRenderer.MinimumChartSize_C23 = 4;
			meshRenderer.AutoUVMaxDistance_C23 = 0.5f;
			meshRenderer.AutoUVMaxAngle_C23 = 89.0f;
			meshRenderer.LightmapParameters_C23?.SetValues(0, 0);
		}
	}
}
