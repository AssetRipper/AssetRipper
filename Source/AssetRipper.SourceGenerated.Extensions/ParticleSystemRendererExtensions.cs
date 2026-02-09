using AssetRipper.SourceGenerated.Classes.ClassID_199;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ParticleSystemRendererExtensions
{
	public static ParticleSystemRenderMode GetRenderMode(this IParticleSystemRenderer renderer)
	{
		return renderer.Has_RenderMode_UInt16()
			? renderer.RenderMode_UInt16E
			: renderer.RenderMode_Int32E;
	}

	public static ParticleSystemSortMode GetSortMode(this IParticleSystemRenderer renderer)
	{
		return renderer.Has_SortMode_Byte()
			? renderer.SortMode_ByteE
			: renderer.Has_SortMode_UInt16()
				? renderer.SortMode_UInt16E
				: (ParticleSystemSortMode)renderer.SortMode_Int32;
	}

	public static ParticleSystemRenderSpace GetRenderAlignment(this IParticleSystemRenderer renderer)
	{
		return renderer.Has_RenderAlignment()
			? renderer.RenderAlignmentE
			: renderer.GetRenderMode() == ParticleSystemRenderMode.Mesh
				? ParticleSystemRenderSpace.Local
				: ParticleSystemRenderSpace.View;
	}
}
