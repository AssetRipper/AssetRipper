using AssetRipper.Core.Classes.ParticleSystemRenderer;
using AssetRipper.Core.Classes.SpriteRenderer;
using AssetRipper.SourceGenerated.Classes.ClassID_199;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ParticleSystemRendererExtensions
	{
		public static ParticleSystemRenderMode GetRenderMode(this IParticleSystemRenderer renderer)
		{
			return renderer.Has_RenderMode_C199_UInt16()
				? (ParticleSystemRenderMode)renderer.RenderMode_C199_UInt16
				: (ParticleSystemRenderMode)renderer.RenderMode_C199_Int32;
		}

		public static ParticleSystemSortMode GetSortMode(this IParticleSystemRenderer renderer)
		{
			return renderer.Has_SortMode_C199_Byte()
				? (ParticleSystemSortMode)renderer.SortMode_C199_Byte
				: renderer.Has_SortMode_C199_UInt16()
					? (ParticleSystemSortMode)renderer.SortMode_C199_UInt16
					: (ParticleSystemSortMode)renderer.SortMode_C199_Int32;
		}

		public static ParticleSystemRenderSpace GetRenderAlignment(this IParticleSystemRenderer renderer)
		{
			return renderer.Has_RenderAlignment_C199()
				? (ParticleSystemRenderSpace)renderer.RenderAlignment_C199
				: renderer.GetRenderMode() == ParticleSystemRenderMode.Mesh
					? ParticleSystemRenderSpace.Local
					: ParticleSystemRenderSpace.View;
		}

		public static SpriteMaskInteraction GetMaskInteraction(this IParticleSystemRenderer renderer)
		{
			return (SpriteMaskInteraction)renderer.MaskInteraction_C199;
		}
	}
}
