using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Utils;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IMeshBlendShape : IAsset
	{
		/// <summary>
		/// Less than 4.3
		/// </summary>
		Utf8StringBase Name { get; }
		uint FirstVertex { get; set; }
		uint VertexCount { get; set; }
		bool HasNormals { get; set; }
		bool HasTangents { get; set; }
		/// <summary>
		/// Less than 4.3
		/// </summary>
		IVector3f AabbMinDelta { get; }
		/// <summary>
		/// Less than 4.3
		/// </summary>
		IVector3f AabbMaxDelta { get; }
	}

	public static class MeshBlendShapeExtensions
	{
		public static bool IsCRCMatch(this IMeshBlendShape blendShape, uint crc)
		{
			if (blendShape.Name is null)
				return false;
			else
				return CrcUtils.VerifyDigestUTF8(blendShape.Name, crc);
		}
	}
}
