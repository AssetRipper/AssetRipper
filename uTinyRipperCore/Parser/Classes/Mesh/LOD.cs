using System.Collections.Generic;

namespace uTinyRipper.Classes.Meshes
{
	public struct LOD : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_meshData = reader.ReadAssetArray<MeshData>();
			VertexCount = reader.ReadInt32();
			NewVertexStart = reader.ReadInt32();
			MeshError = reader.ReadSingle();
			m_morphToVertex = reader.ReadUInt16Array();
		}

		public IReadOnlyList<MeshData> MeshData => m_meshData;
		public int VertexCount { get; private set; }
		public int NewVertexStart { get; private set; }
		public float MeshError { get; private set; }
		public IReadOnlyList<ushort> MorphToVertex => m_morphToVertex;

		private MeshData[] m_meshData;
		private ushort[] m_morphToVertex;
	}
}
