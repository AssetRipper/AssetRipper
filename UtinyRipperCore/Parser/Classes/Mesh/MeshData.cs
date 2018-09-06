using System.Collections.Generic;

namespace UtinyRipper.Classes.Meshes
{
	public struct MeshData : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_faces = reader.ReadArray<Face>();
			m_strips = reader.ReadUInt16Array();
		}
		
		public IReadOnlyList<Face> Faces => m_faces;

		private Face[] m_faces;
		private ushort[] m_strips;
	}
}
