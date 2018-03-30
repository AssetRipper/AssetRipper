using System.Collections.Generic;

namespace UtinyRipper.Classes.Meshes
{
	public struct LOD : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_meshData = stream.ReadArray<MeshData>();
			VertexCount = stream.ReadInt32();
			NewVertexStart = stream.ReadInt32();
			MeshError = stream.ReadSingle();
			m_morphToVertex = stream.ReadUInt16Array();
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
