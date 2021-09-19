using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace AssetRipper.Library.Exporters.Meshes
{
	using VERTEX_P = VertexBuilder<VertexPosition, VertexEmpty, VertexEmpty>;
	using VERTEX_PN = VertexBuilder<VertexPositionNormal, VertexEmpty, VertexEmpty>;
	using VERTEX_PNT = VertexBuilder<VertexPositionNormalTangent, VertexEmpty, VertexEmpty>;

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
			bool hasNormals = mesh.Normals != null && mesh.Normals.Length == mesh.Vertices.Length;
			bool hasTangents = hasNormals && mesh.Tangents != null && mesh.Tangents.Length == mesh.Vertices.Length;
			
			var sceneBuilder = new SceneBuilder();
			var material = new MaterialBuilder("material");

			if (hasTangents)
			{
				var meshBuilder = VERTEX_PNT.CreateCompatibleMesh(mesh.Name);
				var primitiveBuilder = meshBuilder.UsePrimitive(material);
				for (int j = 0; j < mesh.Indices.Count; j += 3)
				{
					primitiveBuilder.AddTriangle(GetVertex_PNT(mesh, mesh.Indices[j]), GetVertex_PNT(mesh, mesh.Indices[j + 1]), GetVertex_PNT(mesh, mesh.Indices[j + 2]));
				}
				sceneBuilder.AddRigidMesh(meshBuilder, System.Numerics.Matrix4x4.Identity);
			}
			else if (hasNormals)
			{
				var meshBuilder = VERTEX_PN.CreateCompatibleMesh(mesh.Name);
				var primitiveBuilder = meshBuilder.UsePrimitive(material);
				for (int j = 0; j < mesh.Indices.Count; j += 3)
				{
					primitiveBuilder.AddTriangle(GetVertex_PN(mesh, mesh.Indices[j]), GetVertex_PN(mesh, mesh.Indices[j + 1]), GetVertex_PN(mesh, mesh.Indices[j + 2]));
				}
				sceneBuilder.AddRigidMesh(meshBuilder, System.Numerics.Matrix4x4.Identity);
			}
			else
			{
				var meshBuilder = VERTEX_P.CreateCompatibleMesh(mesh.Name);
				var primitiveBuilder = meshBuilder.UsePrimitive(material);
				for (int j = 0; j < mesh.Indices.Count; j += 3)
				{
					primitiveBuilder.AddTriangle(GetVertex_P(mesh, mesh.Indices[j]), GetVertex_P(mesh, mesh.Indices[j + 1]), GetVertex_P(mesh, mesh.Indices[j + 2]));
				}
				sceneBuilder.AddRigidMesh(meshBuilder, System.Numerics.Matrix4x4.Identity);
			}

			var model = sceneBuilder.ToGltf2();

			//Write settings can be used in the write glb method if desired
			//var writeSettings = new SharpGLTF.Schema2.WriteSettings();
			
			return model.WriteGLB().ToArray();
		}

		private static VERTEX_P GetVertex_P(Mesh mesh, uint index)
		{
			return new VERTEX_P(new VertexPosition(mesh.Vertices[index]));
		}
		private static VERTEX_PN GetVertex_PN(Mesh mesh, uint index)
		{
			return new VERTEX_PN(new VertexPositionNormal(mesh.Vertices[index], mesh.Normals[index]));
		}
		private static VERTEX_PNT GetVertex_PNT(Mesh mesh, uint index)
		{
			return new VERTEX_PNT(new VertexPositionNormalTangent(mesh.Vertices[index], mesh.Normals[index], mesh.Tangents[index]));
		}
	}
}
