using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_43;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MeshFilterExtensions
	{
		public static IMesh GetMesh(this IMeshFilter meshFilter)
		{
			return meshFilter.Mesh_C33.GetAsset(meshFilter.SerializedFile);
		}
	}
}
