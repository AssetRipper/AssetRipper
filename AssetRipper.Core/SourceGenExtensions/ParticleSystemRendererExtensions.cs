using AssetRipper.SourceGenerated.Classes.ClassID_199;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ParticleSystemRendererExtensions
	{
		public static ParticleSystemRenderMode GetRenderMode(this IParticleSystemRenderer renderer)
		{
			return renderer.Has_RenderMode_C199_UInt16()
				? renderer.RenderMode_C199_UInt16E
				: renderer.RenderMode_C199_Int32E;
		}

		public static ParticleSystemSortMode GetSortMode(this IParticleSystemRenderer renderer)
		{
			return renderer.Has_SortMode_C199_Byte()
				? renderer.SortMode_C199_ByteE
				: renderer.Has_SortMode_C199_UInt16()
					? renderer.SortMode_C199_UInt16E
					: (ParticleSystemSortMode)renderer.SortMode_C199_Int32;
		}

		public static ParticleSystemRenderSpace GetRenderAlignment(this IParticleSystemRenderer renderer)
		{
			return renderer.Has_RenderAlignment_C199()
				? renderer.RenderAlignment_C199E
				: renderer.GetRenderMode() == ParticleSystemRenderMode.Mesh
					? ParticleSystemRenderSpace.Local
					: ParticleSystemRenderSpace.View;
		}
	}
}
