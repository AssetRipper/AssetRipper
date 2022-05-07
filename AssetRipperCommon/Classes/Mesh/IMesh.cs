﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;

namespace AssetRipper.Core.Classes.Mesh
{
	//Partially Implemented
	public interface IMesh : INamedObject
	{
		IVariableBoneCountWeights VariableBoneCountWeights { get; }
		ICompressedMesh CompressedMesh { get; }
		IAABB LocalAABB { get; }
		byte[] BakedConvexCollisionMesh { get; set; }
		byte[] BakedTriangleCollisionMesh { get; set; }
		IStreamingInfo StreamData { get; }
	}

	public static class MeshExtensions
	{
		public static bool IsCombinedMesh(this IMesh mesh) => mesh?.NameString == "Combined Mesh (root scene)";
	}
}
