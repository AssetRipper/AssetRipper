using AssetRipper.SourceGenerated.Subclasses.BlendShapeData;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShapeChannel;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class BlendShapeDataExtensions
	{
		public static string? FindShapeNameByCRC(this IBlendShapeData blendShapeData, uint crc)
		{
			foreach (MeshBlendShapeChannel blendChannel in blendShapeData.Channels)
			{
				if (blendChannel.NameHash == crc)
				{
					return blendChannel.Name.String;
				}
			}
			return null;
		}
	}
}
