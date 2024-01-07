using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.UnityProjects.Meshes
{
	public sealed class GlbMeshExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IMesh mesh && mesh.IsSet())
			{
				exportCollection = new GlbExportCollection(this, asset);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			byte[] data = ExportBinary((IMesh)asset);
			if (data.Length == 0)
			{
				return false;
			}
			File.WriteAllBytes(path, data);
			return true;
		}

		private static byte[] ExportBinary(IMesh mesh)
		{
			SceneBuilder sceneBuilder = new();
			MaterialBuilder material = new MaterialBuilder("DefaultMaterial");

			AddMeshToScene(sceneBuilder, material, mesh);

			SharpGLTF.Schema2.WriteSettings writeSettings = new();

			return sceneBuilder.ToGltf2().WriteGLB(writeSettings).ToArray();
		}

		private static bool AddMeshToScene(SceneBuilder sceneBuilder, MaterialBuilder material, IMesh mesh)
		{
			if (MeshData.TryMakeFromMesh(mesh, out MeshData meshData))
			{
				NodeBuilder rootNodeForMesh = new NodeBuilder(mesh.Name);
				sceneBuilder.AddNode(rootNodeForMesh);

				(ISubMesh, MaterialBuilder)[] subMeshes = new (ISubMesh, MaterialBuilder)[1];

				for (int submeshIndex = 0; submeshIndex < meshData.Mesh.SubMeshes.Count; submeshIndex++)
				{
					ISubMesh subMesh = meshData.Mesh.SubMeshes[submeshIndex];
					subMeshes[0] = (subMesh, material);
					
					IMeshBuilder<MaterialBuilder> subMeshBuilder = GlbSubMeshBuilder.BuildSubMeshes(subMeshes, meshData, Transformation.Identity, Transformation.Identity);
					NodeBuilder subMeshNode = rootNodeForMesh.CreateNode($"SubMesh_{submeshIndex}");
					sceneBuilder.AddRigidMesh(subMeshBuilder, subMeshNode);
				}
				return true;
			}
			return false;
		}
	}
}
