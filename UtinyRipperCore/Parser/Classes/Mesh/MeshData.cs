using System.Collections.Generic;

namespace UtinyRipper.Classes.Meshes
{
	public struct MeshData : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_faces = stream.ReadArray<Face>();
			m_strips = stream.ReadUInt16Array();
		}
		
		public IReadOnlyList<Face> Faces => m_faces;

		private Face[] m_faces;
		private ushort[] m_strips;
	}
}
