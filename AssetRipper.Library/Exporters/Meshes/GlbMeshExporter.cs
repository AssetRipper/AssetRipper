using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Transformations;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Configuration;
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
			return new GlbMeshExportCollection(this, asset);
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IMesh mesh && mesh.IsSet();
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			byte[] data = ExportBinary((IMesh)asset);
			if (data == null || data.Length == 0)
			{
				return false;
			}

			using FileStream fileStream = File.Create(path);
			fileStream.Write(data);
			return true;
		}

		private static byte[] ExportBinary(IMesh mesh)
		{
			SceneBuilder sceneBuilder = new();
			MaterialBuilder material = new MaterialBuilder("material");

			AddMeshToScene(sceneBuilder, material, mesh);

			SharpGLTF.Schema2.WriteSettings writeSettings = new();
			writeSettings.Validation = SharpGLTF.Validation.ValidationMode.Skip; //Required due to non-invertible and non-decomposeable transforms

			return sceneBuilder.ToGltf2().WriteGLB(writeSettings).ToArray();
		}

		private static bool AddMeshToScene(SceneBuilder sceneBuilder, MaterialBuilder material, IMesh mesh)
		{
			if (MeshData.TryMakeFromMesh(mesh, out MeshData meshData))
			{
				NodeBuilder rootNodeForMesh = new NodeBuilder(mesh.NameString);
				//rootNodeForMesh.LocalMatrix = Matrix4x4.Identity; //Local transform can be changed if desired
				sceneBuilder.AddNode(rootNodeForMesh);

				for (int submeshIndex = 0; submeshIndex < meshData.Mesh.SubMeshes_C43.Count; submeshIndex++)
				{
					ISubMesh subMesh = meshData.Mesh.SubMeshes_C43[submeshIndex];
					IMeshBuilder<MaterialBuilder> subMeshBuilder = GlbSubMeshBuilder.BuildSubMesh(material, meshData, subMesh, Transformation.IdentityWithInvertedX, Transformation.IdentityWithInvertedX);
					NodeBuilder subMeshNode = rootNodeForMesh.CreateNode($"SubMesh_{submeshIndex}");
					sceneBuilder.AddRigidMesh(subMeshBuilder, subMeshNode);
				}
				return true;
			}
			return false;
		}
	}
}
