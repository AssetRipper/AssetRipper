using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using System;
using System.Linq;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class CollisionMeshData : IAssetReadable, IAssetWritable
	{
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

		public byte[] BakedConvexCollisionMesh { get; set; } = Array.Empty<byte>();
		public byte[] BakedTriangleCollisionMesh { get; set; } = Array.Empty<byte>();
	}
}
