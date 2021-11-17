using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using MeshSharp;
using MeshSharp.Elements;
using MeshSharp.FBX;
using MeshSharp.OBJ;
using MeshSharp.STL;
using System;
using System.IO;
using System.Linq;

namespace AssetRipper.Library.Exporters.Meshes
{
	class UnifiedMeshExporter : BaseMeshExporter
	{
		public UnifiedMeshExporter(LibraryConfiguration configuration) : base(configuration)
		{
			BinaryExport = true;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, ExportFormat.GetFileExtension());
		}

		public override bool IsHandle(Mesh mesh)
		{
			return IsSupported(ExportFormat) && HasValidMeshData(mesh);
		}

		public static bool HasValidMeshData(Mesh mesh)
		{
			return mesh != null &&
				mesh.Vertices != null &&
				mesh.Vertices.Length > 0 &&
				mesh.Indices != null &&
				mesh.Indices.Count > 0 &&
				mesh.Indices.Count % 3 == 0;
		}

		public static bool IsSupported(MeshExportFormat format) => format switch
		{
			MeshExportFormat.FbxPrimitive => true,
			//MeshExportFormat.StlBinary => true,
			//MeshExportFormat.StlAscii => true,
			//MeshExportFormat.Obj => true,
			_ => false,
		};

		public override byte[] ExportBinary(Mesh mesh)
		{
			Scene scene = ConvertToScene(mesh);
			using MemoryStream memoryStream = new MemoryStream();
			switch (ExportFormat)
			{
				case MeshExportFormat.FbxPrimitive:
					FbxWriter.WriteAscii(memoryStream, scene, MeshSharp.FBX.FbxVersion.v7400);
					break;
				case MeshExportFormat.StlBinary:
					StlWriter.WriteBinary(memoryStream, scene);
					break;
				case MeshExportFormat.StlAscii:
					StlWriter.WriteAscii(memoryStream, scene);
					break;
				case MeshExportFormat.Obj:
					ObjWriter.Write(memoryStream, scene);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(ExportFormat));
			}
			return memoryStream.ToArray();
		}

		private static Scene ConvertToScene(Mesh unityMesh)
		{
			var outputMesh = ConvertToMeshSharpMesh(unityMesh);
			Scene scene = new Scene();
			scene.Name = unityMesh.Name;
			Node node = new Node();
			node.Name = unityMesh.Name;
			node.Children.Add(outputMesh);
			scene.Nodes.Add(node);
			return scene;
		}

		private static MeshSharp.Elements.Geometries.Mesh ConvertToMeshSharpMesh(Mesh unityMesh)
		{
			bool hasVertexNormals = unityMesh.Normals != null && unityMesh.Normals.Length == unityMesh.Vertices.Length;
			bool hasTangents = unityMesh.Tangents != null && unityMesh.Tangents.Length == unityMesh.Vertices.Length;
			bool hasUV0 = unityMesh.UV0 != null && unityMesh.UV0.Length == unityMesh.Vertices.Length;
			bool hasUV1 = unityMesh.UV1 != null && unityMesh.UV1.Length == unityMesh.Vertices.Length;
			bool hasColors = unityMesh.Colors != null && unityMesh.Colors.Length == unityMesh.Vertices.Length;
			//Logger.Info($"VertexNormals: {hasVertexNormals}");
			//Logger.Info($"Tangents: {hasTangents}");
			//Logger.Info($"UV 0: {hasUV0}");
			//Logger.Info($"UV 1: {hasUV1}");
			//Logger.Info($"Colors: {hasColors}");
			var outputMesh = new MeshSharp.Elements.Geometries.Mesh();
			outputMesh.Name = unityMesh.Name;

			//Vertices
			foreach (var vertex in unityMesh.Vertices)
			{
				outputMesh.Vertices.Add(Convert(vertex));
			}

			//Polygons
			for (int i = 0; i + 2 < unityMesh.Indices.Count; i += 3)
			{
				outputMesh.Polygons.Add(new Triangle(unityMesh.Indices[i], unityMesh.Indices[i + 1], unityMesh.Indices[i + 2]));
			}

			//Normals
			var normalElement = new MeshSharp.Elements.Geometries.Layers.LayerElementNormal(outputMesh);
			outputMesh.Layers.Add(normalElement);
			if (hasVertexNormals)
			{
				normalElement.MappingInformationType = MeshSharp.Elements.Geometries.Layers.MappingMode.ByPolygonVertex;
				normalElement.ReferenceInformationType = MeshSharp.Elements.Geometries.Layers.ReferenceMode.Direct;
				foreach(var polygon in outputMesh.Polygons)
				{
					foreach(uint index in polygon.Indices)
					{
						normalElement.Normals.Add(Convert(unityMesh.Normals[index]));
					}
				}
			}
			else
			{
				normalElement.CalculateFlatNormals();
			}

			//Tangents
			if (hasTangents)
			{
				var tangentElement = new MeshSharp.Elements.Geometries.Layers.LayerElementTangent(outputMesh);
				outputMesh.Layers.Add(tangentElement);
				tangentElement.MappingInformationType = MeshSharp.Elements.Geometries.Layers.MappingMode.ByPolygonVertex;
				tangentElement.ReferenceInformationType = MeshSharp.Elements.Geometries.Layers.ReferenceMode.Direct;
				tangentElement.Tangents.AddRange(unityMesh.Tangents.Select(t => Convert((Vector3f)t)));
				//We're excluding W here because it's not supported by MeshSharp
				//For Unity, the tangent W coordinate denotes the direction of the binormal vector and is always 1 or -1
				//https://docs.unity3d.com/ScriptReference/Mesh-tangents.html
			}

			//UV
			if (hasUV0)
			{
				var uv0Element = new MeshSharp.Elements.Geometries.Layers.LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv0Element);
				uv0Element.MappingInformationType = MeshSharp.Elements.Geometries.Layers.MappingMode.ByPolygonVertex;
				uv0Element.ReferenceInformationType = MeshSharp.Elements.Geometries.Layers.ReferenceMode.IndexToDirect;
				uv0Element.UV.AddRange(unityMesh.UV0.Select(v => Convert(v)));
				uv0Element.UVIndex.AddRange(Enumerable.Range(0, unityMesh.UV0.Length));
			}
			if (hasUV1)
			{
				var uv1Element = new MeshSharp.Elements.Geometries.Layers.LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv1Element);
				uv1Element.MappingInformationType = MeshSharp.Elements.Geometries.Layers.MappingMode.ByPolygonVertex;
				uv1Element.ReferenceInformationType = MeshSharp.Elements.Geometries.Layers.ReferenceMode.IndexToDirect;
				uv1Element.UV.AddRange(unityMesh.UV1.Select(v => Convert(v)));
				uv1Element.UVIndex.AddRange(Enumerable.Range(0, unityMesh.UV1.Length));
			}

			//Colors
			if (hasColors)
			{
				var colorElement = new MeshSharp.Elements.Geometries.Layers.LayerElementVertexColor(outputMesh);
				outputMesh.Layers.Add(colorElement);
				colorElement.MappingInformationType = MeshSharp.Elements.Geometries.Layers.MappingMode.ByPolygonVertex;
				colorElement.ReferenceInformationType = MeshSharp.Elements.Geometries.Layers.ReferenceMode.IndexToDirect;
				colorElement.Colors.AddRange(unityMesh.Colors.Select(v => Convert(v)));
				colorElement.ColorIndex.AddRange(Enumerable.Range(0, unityMesh.Colors.Length));
			}

			return outputMesh;
		}

		private static XY Convert(Vector2f vector) => new XY(vector.X, vector.Y);
		private static XYZ Convert(Vector3f vector) => new XYZ(vector.X, vector.Y, vector.Z);
		private static XYZM Convert(Vector4f vector) => new XYZM(vector.X, vector.Y, vector.Z, vector.W);
		private static XYZM Convert(ColorRGBAf vector) => new XYZM(vector.R, vector.G, vector.B, vector.A);
		private static XYZM Convert(ColorRGBA32 vector) => Convert((ColorRGBAf)vector);
	}
}
