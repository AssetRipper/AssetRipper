using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Transformations;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.IO;

namespace AssetRipper.Library.Exporters.Meshes
{
	public sealed class GlbMeshExporter : BinaryAssetExporter
	{
		public GlbMeshExporter() : base() { }

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new GlbExportCollection(this, asset);
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IMesh mesh && mesh.IsSet();
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
				NodeBuilder rootNodeForMesh = new NodeBuilder(mesh.NameString);
				sceneBuilder.AddNode(rootNodeForMesh);

				(ISubMesh, MaterialBuilder)[] subMeshes = new (ISubMesh, MaterialBuilder)[1];

				for (int submeshIndex = 0; submeshIndex < meshData.Mesh.SubMeshes_C43.Count; submeshIndex++)
				{
					ISubMesh subMesh = meshData.Mesh.SubMeshes_C43[submeshIndex];
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
