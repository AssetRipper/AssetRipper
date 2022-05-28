using AssetRipper.Core.Classes.Misc;
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
			if(renderer is IMeshRenderer meshRenderer)
			{
				return meshRenderer.Materials_C23;
			}
			else if (renderer is IParticleRenderer particleRenderer)
			{
				return particleRenderer.Materials_C26;
			}
			else if (renderer is ITrailRenderer trailRenderer)
			{
				return trailRenderer.Materials_C96;
			}
			else if (renderer is ILineRenderer lineRenderer)
			{
				return lineRenderer.Materials_C120;
			}
			else if (renderer is ISkinnedMeshRenderer skinnedMeshRenderer)
			{
				return skinnedMeshRenderer.Materials_C137;
			}
			else if (renderer is IClothRenderer clothRenderer)
			{
				return clothRenderer.Materials_C161;
			}
			else if (renderer is IParticleSystemRenderer particleSystemRenderer)
			{
				return particleSystemRenderer.Materials_C199;
			}
			else if (renderer is ISpriteRenderer spriteRenderer)
			{
				return spriteRenderer.Materials_C212;
			}
			else if (renderer is IBillboardRenderer billboardRenderer)
			{
				return billboardRenderer.Materials_C227;
			}
			else if (renderer is ISpriteMask spriteMask)
			{
				return spriteMask.Materials_C331;
			}
			else if (renderer is ITilemapRenderer tilemapRenderer)
			{
				return tilemapRenderer.Materials_C483693784;
			}
			else if (renderer is ISpriteShapeRenderer spriteShapeRenderer)
			{
				return spriteShapeRenderer.Materials_C1971053207;
			}
			else if (renderer is IVFXRenderer vfxRenderer)
			{
				return vfxRenderer.Materials_C73398921;
			}
			else
			{
				throw new NotSupportedException(renderer.GetType().FullName);
			}
		}
	}
}
