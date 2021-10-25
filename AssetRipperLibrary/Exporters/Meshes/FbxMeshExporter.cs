using AssetRipper.Core;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using MeshIO;
using MeshIO.Elements;
using MeshIO.FBX.Converters;
using System.IO;

namespace AssetRipper.Library.Exporters.Meshes
{
	class FbxMeshExporter : BaseMeshExporter
	{
		public FbxMeshExporter(LibraryConfiguration configuration) : base(configuration) { }

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
			IFbxConverter converter = FbxConverterBase.GetConverter(scene, MeshIO.FBX.FbxVersion.v7400);
			var rootNode = converter.ToRootNode();
			using MemoryStream memoryStream = new MemoryStream();
			//using (FbxBinaryWriter writer = new FbxBinaryWriter(memoryStream));
			//writer.Write(rootNode);
			return base.ExportBinary(mesh);
		}

		private static Scene ConvertToScene(Mesh unityMesh)
		{
			var outputMesh = new MeshIO.Elements.Geometries.Mesh();
			outputMesh.Name = unityMesh.Name;
			foreach(var vertex in unityMesh.Vertices)
			{
				outputMesh.Vertices.Add(Convert(vertex));
			}
			for(int i = 0; i + 2 < unityMesh.Indices.Count; i += 3)
			{
				outputMesh.Polygons.Add(new Triangle(unityMesh.Indices[i], unityMesh.Indices[i + 1], unityMesh.Indices[i + 2]));
			}
			return default;
		}

		private static XYZ Convert(Vector3f vector) => new XYZ(vector.X, vector.Y, vector.Z);
	}
}
