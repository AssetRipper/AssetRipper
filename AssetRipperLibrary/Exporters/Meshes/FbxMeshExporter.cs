using AssetRipper.Core;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using MeshSharp;
using MeshSharp.Elements;
using MeshSharp.FBX;
using System.IO;

namespace AssetRipper.Library.Exporters.Meshes
{
	class FbxMeshExporter : BaseMeshExporter
	{
		public FbxMeshExporter(LibraryConfiguration configuration) : base(configuration)
		{
			BinaryExport = true; //Technically this exports in the ascii format right now, but a memory stream was easier to use.
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, "fbx");
		}

		public override bool IsHandle(Mesh mesh)
		{
			return ExportFormat == MeshExportFormat.FbxPrimitive;
		}

		public override byte[] ExportBinary(Mesh mesh)
		{
			Scene scene = ConvertToScene(mesh);
			using MemoryStream memoryStream = new MemoryStream();
			FbxWriter.WriteAscii(memoryStream, scene, MeshSharp.FBX.FbxVersion.v7400);
			return memoryStream.ToArray();
		}

		private static Scene ConvertToScene(Mesh unityMesh)
		{
			var outputMesh = new MeshSharp.Elements.Geometries.Mesh();
			outputMesh.Name = unityMesh.Name;
			foreach(var vertex in unityMesh.Vertices)
			{
				outputMesh.Vertices.Add(Convert(vertex));
			}
			for(int i = 0; i + 2 < unityMesh.Indices.Count; i += 3)
			{
				outputMesh.Polygons.Add(new Triangle(unityMesh.Indices[i], unityMesh.Indices[i + 1], unityMesh.Indices[i + 2]));
			}
			Scene scene = new Scene();
			scene.Name = unityMesh.Name;
			Node node = new Node();
			node.Name = unityMesh.Name;
			node.Children.Add(outputMesh);
			scene.Nodes.Add(node);
			return scene;
		}

		private static XYZ Convert(Vector3f vector) => new XYZ(vector.X, vector.Y, vector.Z);
	}
}
