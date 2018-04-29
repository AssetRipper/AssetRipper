using System.Collections.Generic;

namespace UtinyRipper.Classes.Meshes
{
	public struct CollisionMeshData : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_bakedConvexCollisionMesh = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);
			m_bakedTriangleCollisionMesh = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);
		}

		public IReadOnlyList<byte> BakedConvexCollisionMesh => m_bakedConvexCollisionMesh;
		public IReadOnlyList<byte> BakedTriangleCollisionMesh => m_bakedTriangleCollisionMesh;

		private byte[] m_bakedConvexCollisionMesh;
		private byte[] m_bakedTriangleCollisionMesh;
	}
}
