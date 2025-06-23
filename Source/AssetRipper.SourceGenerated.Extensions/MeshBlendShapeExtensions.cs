using AssetRipper.Checksum;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShape;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MeshBlendShapeExtensions
{
	public static bool IsCRCMatch(this IMeshBlendShape blendShape, uint crc)
	{
		return blendShape.Name_R is not null && Crc32Algorithm.MatchUTF8(blendShape.Name_R.String, crc);
	}
}
