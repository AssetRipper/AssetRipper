using AssetRipper.Core.Project;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;
using AssetRipper.Core.Math;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public struct Blend2dDataConstant : IAssetReadable, IYAMLExportable
	{
		public Blend2dDataConstant(ObjectReader reader)
		{
			m_ChildPositionArray = reader.ReadVector2Array();
			m_ChildMagnitudeArray = reader.ReadSingleArray();
			m_ChildPairVectorArray = reader.ReadVector2Array();
			m_ChildPairAvgMagInvArray = reader.ReadSingleArray();

			int numNeighbours = reader.ReadInt32();
			m_ChildNeighborListArray = new MotionNeighborList[numNeighbours];
			for (int i = 0; i < numNeighbours; i++)
			{
				m_ChildNeighborListArray[i] = new MotionNeighborList(reader);
			}
		}

		public void Read(AssetReader reader)
		{
			m_ChildPositionArray = reader.ReadAssetArray<Vector2f>();
			m_ChildMagnitudeArray = reader.ReadSingleArray();
			m_ChildPairVectorArray = reader.ReadAssetArray<Vector2f>();
			m_ChildPairAvgMagInvArray = reader.ReadSingleArray();
			m_ChildNeighborListArray = reader.ReadAssetArray<MotionNeighborList>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public Vector2f[] m_ChildPositionArray { get; set; }
		public float[] m_ChildMagnitudeArray { get; set; }
		public Vector2f[] m_ChildPairVectorArray { get; set; }
		public float[] m_ChildPairAvgMagInvArray { get; set; }
		public MotionNeighborList[] m_ChildNeighborListArray { get; set; }
	}
}
