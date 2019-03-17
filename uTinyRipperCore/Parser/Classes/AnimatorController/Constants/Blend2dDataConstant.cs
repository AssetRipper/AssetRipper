using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct Blend2dDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_childPositionArray = reader.ReadAssetArray<Vector2f>();
			m_childMagnitudeArray = reader.ReadSingleArray();
			m_childPairVectorArray = reader.ReadAssetArray<Vector2f>();
			m_childPairAvgMagInvArray = reader.ReadSingleArray();
			m_childNeighborListArray = reader.ReadAssetArray<MotionNeighborList>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<Vector2f> ChildPositionArray => m_childPositionArray;
		public IReadOnlyList<float> ChildMagnitudeArray => m_childMagnitudeArray;
		public IReadOnlyList<Vector2f> ChildPairVectorArray => m_childPairVectorArray;
		public IReadOnlyList<float> ChildPairAvgMagInvArray => m_childPairAvgMagInvArray;
		public IReadOnlyList<MotionNeighborList> ChildNeighborListArray => m_childNeighborListArray;

		private Vector2f[] m_childPositionArray;
		private float[] m_childMagnitudeArray;
		private Vector2f[] m_childPairVectorArray;
		private float[] m_childPairAvgMagInvArray;
		private MotionNeighborList[] m_childNeighborListArray;
	}
}
