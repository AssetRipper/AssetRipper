using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Mesh
{
	//Not Yet Implemented
	public interface IMesh : IHasMeshData, INamedObject
	{
	}

	public static class MeshExtensions
	{
		public static bool IsCombinedMesh(this IMesh mesh) => mesh?.Name == "Combined Mesh (root scene)";
	}
}
