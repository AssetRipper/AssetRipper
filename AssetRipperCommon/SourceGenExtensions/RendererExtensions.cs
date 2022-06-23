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
				IMaterial? material = materialPtr.FindAsset(renderer.SerializedFile);
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

		private static IEnumerable<IPPtr_Material_> GetMaterials(this IRenderer renderer)
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

		public static void ConvertToEditorFormat(this IRenderer renderer)
		{
			switch (renderer)
			{
				case IMeshRenderer meshRenderer:
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
					break;
				case IParticleRenderer particleRenderer:
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
					break;
				case ITrailRenderer trailRenderer:
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
					break;
				case ILineRenderer lineRenderer:
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
					break;
				case ISkinnedMeshRenderer skinnedMeshRenderer:
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
					break;
				case IClothRenderer clothRenderer:
					clothRenderer.ScaleInLightmap_C161 = 1.0f;
					//others absent because cloth renderer not used in Unity 5+
					break;
				case IParticleSystemRenderer particleSystemRenderer:
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
					break;
				case ISpriteRenderer spriteRenderer:
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
					break;
				case IBillboardRenderer billboardRenderer:
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
					break;
				case ISpriteMask spriteMask:
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
					break;
				case ITilemapRenderer tilemapRenderer:
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
					break;
				case ISpriteShapeRenderer spriteShapeRenderer:
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
					break;
				case IVFXRenderer vfxRenderer:
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
					break;
				default:
					throw new NotSupportedException(renderer.GetType().FullName);
			}
		}
	}
}
