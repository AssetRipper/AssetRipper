using System;
using System.Linq;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.Meshes
{
	public struct CollisionMeshData : IAssetReadable, IAssetWritable
	{
		public CollisionMeshData(Version version)
		{
			BakedConvexCollisionMesh = Array.Empty<byte>();
			BakedTriangleCollisionMesh = Array.Empty<byte>();
		}

		public CollisionMeshData Convert(IExportContainer container)
		{
			CollisionMeshData instance = new CollisionMeshData();
			instance.BakedConvexCollisionMesh = BakedConvexCollisionMesh.ToArray();
			instance.BakedTriangleCollisionMesh = BakedTriangleCollisionMesh.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			BakedConvexCollisionMesh = reader.ReadByteArray();
			reader.AlignStream();
			BakedTriangleCollisionMesh = reader.ReadByteArray();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(BakedConvexCollisionMesh);
			writer.AlignStream();
			writer.Write(BakedTriangleCollisionMesh);
			writer.AlignStream();
		}

		public byte[] BakedConvexCollisionMesh { get; set; }
		public byte[] BakedTriangleCollisionMesh { get; set; }
	}
}
