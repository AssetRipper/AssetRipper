using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Library.Configuration;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace AssetRipper.Library.Exporters.Meshes
{
	using VERTEX = VertexBuilder<VertexPosition, VertexEmpty, VertexEmpty>;

	public class GlbMeshExporter : BaseMeshExporter
	{
		public GlbMeshExporter(LibraryConfiguration configuration) : base(configuration) => BinaryExport = true;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Core.Classes.Object.Object asset)
		{
			return new AssetExportCollection(this, asset, "glb");
		}

		public override bool IsHandle(Mesh mesh)
		{
			return ExportFormat == MeshExportFormat.GlbPrimitive && 
				mesh.Vertices != null && 
				mesh.Vertices.Length > 0 && 
				mesh.Indices != null && 
				mesh.Indices.Count > 0 && 
				mesh.Indices.Count % 3 == 0;
		}

		public override byte[] ExportBinary(Mesh mesh)
		{
			var meshBuilder = VERTEX.CreateCompatibleMesh(mesh.Name);
			var material = new MaterialBuilder("material");
			var primitiveBuilder = meshBuilder.UsePrimitive(material);
			for (int j = 0; j < mesh.Indices.Count; j += 3)
			{
				primitiveBuilder.AddTriangle(GetVertex(mesh, mesh.Indices[j]), GetVertex(mesh, mesh.Indices[j + 1]), GetVertex(mesh, mesh.Indices[j + 2]));
			}

			var sceneBuilder = new SceneBuilder();

			sceneBuilder.AddRigidMesh(meshBuilder, System.Numerics.Matrix4x4.Identity);

			var model = sceneBuilder.ToGltf2();

			//Write settings can be used in the write glb method if desired
			//var writeSettings = new SharpGLTF.Schema2.WriteSettings();
			
			return model.WriteGLB().ToArray();
		}

		private static VERTEX GetVertex(Mesh mesh, uint index)
		{
			return new VERTEX(new VertexPosition(mesh.Vertices[index]));
		}
	}
}
