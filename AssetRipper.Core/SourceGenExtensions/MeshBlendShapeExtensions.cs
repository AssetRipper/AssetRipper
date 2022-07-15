using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShape;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MeshBlendShapeExtensions
	{
		public static bool IsCRCMatch(this IMeshBlendShape blendShape, uint crc)
		{
			return blendShape.Name_R is not null && CrcUtils.VerifyDigestUTF8(blendShape.Name_R.String, crc);
		}
	}
}
