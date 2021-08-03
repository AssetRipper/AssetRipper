using AssetRipper.Core.Math;

namespace AssetRipper.Core.Imported
{
	public class ImportedVertex
	{
		public Vector3f Vertex { get; set; }
		public Vector3f Normal { get; set; }
		public float[][] UV { get; set; }
		public Vector4f Tangent { get; set; }
		public ColorRGBAf Color { get; set; }
		public float[] Weights { get; set; }
		public int[] BoneIndices { get; set; }
	}
}
