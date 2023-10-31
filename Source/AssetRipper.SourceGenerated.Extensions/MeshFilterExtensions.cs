﻿using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_43;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class MeshFilterExtensions
	{
		public static IMesh GetMesh(this IMeshFilter meshFilter)
		{
			return meshFilter.Mesh.GetAsset(meshFilter.Collection);
		}

		public static bool TryGetMesh(this IMeshFilter meshFilter, [NotNullWhen(true)] out IMesh? mesh)
		{
			mesh = meshFilter.MeshP;
			return mesh != null;
		}
	}
}
