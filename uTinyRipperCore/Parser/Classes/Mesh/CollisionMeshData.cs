using System.Collections.Generic;

namespace uTinyRipper.Classes.Meshes
{
	public struct CollisionMeshData : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_bakedConvexCollisionMesh = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
			m_bakedTriangleCollisionMesh = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
		}

		public IReadOnlyList<byte> BakedConvexCollisionMesh => m_bakedConvexCollisionMesh;
		public IReadOnlyList<byte> BakedTriangleCollisionMesh => m_bakedTriangleCollisionMesh;

		private byte[] m_bakedConvexCollisionMesh;
		private byte[] m_bakedTriangleCollisionMesh;
	}
}
