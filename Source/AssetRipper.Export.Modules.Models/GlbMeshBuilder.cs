using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.Modules.Models;

public static class GlbMeshBuilder
{
	public static SceneBuilder Build(IMesh mesh)
	{
		SceneBuilder sceneBuilder = new();
		MaterialBuilder material = new MaterialBuilder("DefaultMaterial");

		AddMeshToScene(sceneBuilder, material, mesh);

		return sceneBuilder;
	}

	private static bool AddMeshToScene(SceneBuilder sceneBuilder, MaterialBuilder material, IMesh mesh)
	{
		if (MeshData.TryMakeFromMesh(mesh, out MeshData meshData))
		{
			NodeBuilder rootNodeForMesh = new NodeBuilder(mesh.Name);
			sceneBuilder.AddNode(rootNodeForMesh);

			(ISubMesh, MaterialBuilder)[] subMeshes = new (ISubMesh, MaterialBuilder)[1];

			for (int submeshIndex = 0; submeshIndex < mesh.SubMeshes.Count; submeshIndex++)
			{
				ISubMesh subMesh = mesh.SubMeshes[submeshIndex];
				subMeshes[0] = (subMesh, material);

				IMeshBuilder<MaterialBuilder> subMeshBuilder = GlbSubMeshBuilder.BuildSubMeshes(subMeshes, mesh.Is16BitIndices(), meshData, Transformation.Identity, Transformation.Identity);
				NodeBuilder subMeshNode = rootNodeForMesh.CreateNode($"SubMesh_{submeshIndex}");
				sceneBuilder.AddRigidMesh(subMeshBuilder, subMeshNode);
			}
			return true;
		}
		return false;
	}
}
