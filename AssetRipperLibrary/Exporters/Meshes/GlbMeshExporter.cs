using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.IO;

namespace AssetRipper.Library.Exporters.Meshes
{
	public class GlbMeshExporter : BinaryAssetExporter
	{
		protected MeshExportFormat ExportFormat { get; set; }
		public GlbMeshExporter(LibraryConfiguration configuration) : base() => ExportFormat = configuration.MeshExportFormat;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, "glb");
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			if (asset is Mesh mesh)
				return IsHandle(mesh);
			else
				return false;
		}

		public bool IsHandle(Mesh mesh)
		{
			return ExportFormat == MeshExportFormat.GlbPrimitive &&
				mesh.Vertices != null &&
				mesh.Vertices.Length > 0 &&
				mesh.Indices != null &&
				mesh.Indices.Count > 0 &&
				mesh.Indices.Count % 3 == 0;
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			byte[] data = ExportBinary((Mesh)asset);
			if (data == null || data.Length == 0)
				return false;

			using FileStream fileStream = File.Create(path);
			fileStream.Write(data);
			return true;
		}

		private byte[] ExportBinary(Mesh mesh)
		{
			bool hasNormals = mesh.Normals != null && mesh.Normals.Length == mesh.Vertices.Length;
			bool hasTangents = hasNormals && mesh.Tangents != null && mesh.Tangents.Length == mesh.Vertices.Length;
			bool hasUV0 = mesh.UV0 != null && mesh.UV0.Length == mesh.Vertices.Length;
			bool hasUV1 = hasUV0 && mesh.UV1 != null && mesh.UV1.Length == mesh.Vertices.Length;

			var sceneBuilder = new SceneBuilder();
			var material = new MaterialBuilder("material");

			if (hasTangents)
			{
				if (hasUV1)
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture2>(mesh, sceneBuilder, material);
				else if (hasUV0)
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture1>(mesh, sceneBuilder, material);
				else
					AddMeshToScene<VertexPositionNormalTangent, VertexEmpty>(mesh, sceneBuilder, material);
			}
			else if (hasNormals)
			{
				if (hasUV1)
					AddMeshToScene<VertexPositionNormal, VertexTexture2>(mesh, sceneBuilder, material);
				else if (hasUV0)
					AddMeshToScene<VertexPositionNormal, VertexTexture1>(mesh, sceneBuilder, material);
				else
					AddMeshToScene<VertexPositionNormal, VertexEmpty>(mesh, sceneBuilder, material);
			}
			else
			{
				if (hasUV1)
					AddMeshToScene<VertexPosition, VertexTexture2>(mesh, sceneBuilder, material);
				else if (hasUV0)
					AddMeshToScene<VertexPosition, VertexTexture1>(mesh, sceneBuilder, material);
				else
					AddMeshToScene<VertexPosition, VertexEmpty>(mesh, sceneBuilder, material);
			}

			var model = sceneBuilder.ToGltf2();

			//Write settings can be used in the write glb method if desired
			//var writeSettings = new SharpGLTF.Schema2.WriteSettings();

			return model.WriteGLB().ToArray();
		}

		private static void AddMeshToScene<TvG, TvM>(Mesh mesh, SceneBuilder sceneBuilder, MaterialBuilder material) where TvG : struct, IVertexGeometry where TvM : struct, IVertexMaterial
		{
			var meshBuilder = VertexBuilder<TvG, TvM, VertexEmpty>.CreateCompatibleMesh(mesh.Name);
			var primitiveBuilder = meshBuilder.UsePrimitive(material);
			for (int j = 0; j < mesh.Indices.Count; j += 3)
			{
				primitiveBuilder.AddTriangle(GetVertex<TvG, TvM>(mesh, mesh.Indices[j]), GetVertex<TvG, TvM>(mesh, mesh.Indices[j + 1]), GetVertex<TvG, TvM>(mesh, mesh.Indices[j + 2]));
			}
			sceneBuilder.AddRigidMesh(meshBuilder, System.Numerics.Matrix4x4.Identity);
		}

		private static VertexBuilder<TvG, TvM, VertexEmpty> GetVertex<TvG, TvM>(Mesh mesh, uint index) where TvG : struct, IVertexGeometry where TvM : struct, IVertexMaterial
		{
			IVertexGeometry geometry;
			if (typeof(TvG) == typeof(VertexPosition))
			{
				geometry = new VertexPosition(mesh.Vertices[index]);
			}
			else if (typeof(TvG) == typeof(VertexPositionNormal))
			{
				geometry = new VertexPositionNormal(mesh.Vertices[index], mesh.Normals[index]);
			}
			else if (typeof(TvG) == typeof(VertexPositionNormalTangent))
			{
				geometry = new VertexPositionNormalTangent(mesh.Vertices[index], mesh.Normals[index], mesh.Tangents[index]);
			}
			else
			{
				geometry = default(TvG);
			}

			IVertexMaterial material;
			if (typeof(TvM) == typeof(VertexTexture1))
			{
				material = new VertexTexture1(mesh.UV0[index]);
			}
			else if (typeof(TvM) == typeof(VertexTexture2))
			{
				material = new VertexTexture2(mesh.UV0[index], mesh.UV1[index]);
			}
			else
			{
				material = default(TvM);
			}

			return new VertexBuilder<TvG, TvM, VertexEmpty>((TvG)geometry, (TvM)material);
		}
	}
}
