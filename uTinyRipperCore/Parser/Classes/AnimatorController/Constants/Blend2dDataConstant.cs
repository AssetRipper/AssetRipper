using System;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct Blend2dDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ChildPositionArray = reader.ReadAssetArray<Vector2f>();
			ChildMagnitudeArray = reader.ReadSingleArray();
			ChildPairVectorArray = reader.ReadAssetArray<Vector2f>();
			ChildPairAvgMagInvArray = reader.ReadSingleArray();
			ChildNeighborListArray = reader.ReadAssetArray<MotionNeighborList>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public Vector2f[] ChildPositionArray { get; set; }
		public float[] ChildMagnitudeArray { get; set; }
		public Vector2f[] ChildPairVectorArray { get; set; }
		public float[] ChildPairAvgMagInvArray { get; set; }
		public MotionNeighborList[] ChildNeighborListArray { get; set; }
	}
}
