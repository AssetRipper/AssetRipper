using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class Blend2dDataConstant : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			m_ChildPositionArray = reader.ReadAssetArray<Vector2f>();
			m_ChildMagnitudeArray = reader.ReadSingleArray();
			m_ChildPairVectorArray = reader.ReadAssetArray<Vector2f>();
			m_ChildPairAvgMagInvArray = reader.ReadSingleArray();
			m_ChildNeighborListArray = reader.ReadAssetArray<MotionNeighborList>();
		}

		public YamlNode ExportYaml(IExportContainer container)
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
