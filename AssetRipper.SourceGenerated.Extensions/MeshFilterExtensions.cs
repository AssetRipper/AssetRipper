using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class MeshFilterExtensions
	{
		public static IMesh GetMesh(this IMeshFilter meshFilter)
		{
			return meshFilter.Mesh_C33.GetAsset(meshFilter.Collection);
		}

		public static bool TryGetMesh(this IMeshFilter meshFilter, [NotNullWhen(true)] out IMesh? mesh)
		{
			return meshFilter.Mesh_C33.TryGetAsset(meshFilter.Collection, out mesh);
		}
	}
}
