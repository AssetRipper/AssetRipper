using AssetRipper.Core;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using MeshSharp;
using MeshSharp.Elements;
using MeshSharp.Elements.Geometries.Layers;
using MeshSharp.FBX;
using MeshSharp.OBJ;
using MeshSharp.PLY;
using MeshSharp.STL;
using System;
using System.IO;
using System.Linq;

namespace AssetRipper.Library.Exporters.Meshes
{
	public class UnifiedMeshExporter : BinaryAssetExporter
	{
		protected MeshExportFormat ExportFormat { get; set; }
		protected MeshCoordinateSpace ExportSpace { get; set; }
		public UnifiedMeshExporter(LibraryConfiguration configuration)
		{
			ExportFormat = configuration.MeshExportFormat;
			ExportSpace = configuration.MeshCoordinateSpace;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, ExportFormat.GetFileExtension());
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			if (asset is Mesh mesh)
				return IsSupported(ExportFormat) && HasValidMeshData(mesh);
			else
				return false;
		}

		public static bool HasValidMeshData(Mesh mesh)
		{
			return mesh != null &&
				mesh.Vertices != null &&
				mesh.Vertices.Length > 0 &&
				mesh.Indices != null &&
				mesh.Indices.Count > 0 &&
				mesh.Indices.Count % 3 == 0 &&
				IsNotLinesOrPoints(mesh);
		}

		private static bool IsNotLinesOrPoints(Mesh mesh)
		{
			if (mesh.AssetUnityVersion.IsLess(4))
				return true;
			foreach (var submesh in mesh.SubMeshes)
			{
				switch (submesh.Topology)
				{
					case MeshTopology.Lines:
					case MeshTopology.LineStrip:
					case MeshTopology.Points:
						return false;
				}
			}
			return true;
		}

		public static bool IsSupported(MeshExportFormat format) => format switch
		{
			MeshExportFormat.FbxPrimitive => true,
			MeshExportFormat.StlBinary => true,
			MeshExportFormat.StlAscii => true,
			MeshExportFormat.Obj => true,
			MeshExportFormat.PlyAscii => true,
			_ => false,
		};

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			byte[] data = ExportBinary((Mesh)asset);
			if (data == null || data.Length == 0)
				return false;

			TaskManager.AddTask(File.WriteAllBytesAsync(path, data));
			return true;
		}

		private byte[] ExportBinary(Mesh mesh)
		{
			Scene scene = ConvertToScene(mesh);
			using MemoryStream memoryStream = new MemoryStream();
			switch (ExportFormat)
			{
				case MeshExportFormat.FbxPrimitive:
					FbxWriter.WriteAscii(memoryStream, scene, FbxVersion.v7400);
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
				case MeshExportFormat.PlyAscii:
					PlyWriter.WriteAscii(memoryStream, scene);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(ExportFormat));
			}
			return memoryStream.ToArray();
		}

		private Scene ConvertToScene(Mesh unityMesh)
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

		private MeshSharp.Elements.Geometries.Mesh ConvertToMeshSharpMesh(Mesh unityMesh)
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
				outputMesh.Vertices.Add(Convert(ToCoordinateSpace(vertex, ExportSpace)));
			}

			//Polygons
			for (int i = 0; i + 2 < unityMesh.Indices.Count; i += 3)
			{
				if (ExportSpace == MeshCoordinateSpace.Right) //Switching to a right handed coordinate system requires reversing the polygon vertex order
					outputMesh.Polygons.Add(new Triangle(unityMesh.Indices[i + 2], unityMesh.Indices[i + 1], unityMesh.Indices[i]));
				else
					outputMesh.Polygons.Add(new Triangle(unityMesh.Indices[i], unityMesh.Indices[i + 1], unityMesh.Indices[i + 2]));
			}

			//Normals
			var normalElement = new LayerElementNormal(outputMesh);
			outputMesh.Layers.Add(normalElement);
			if (hasVertexNormals)
			{
				normalElement.MappingInformationType = MappingMode.ByPolygonVertex;
				normalElement.ReferenceInformationType = ReferenceMode.Direct;
				foreach (var polygon in outputMesh.Polygons)
				{
					foreach (uint index in polygon.Indices)
					{
						normalElement.Normals.Add(Convert(ToCoordinateSpace(unityMesh.Normals[index], ExportSpace)));
					}
				}
			}
			else if (ExportFormat.IsSTL())
			{
				normalElement.CalculateFlatNormals();
			}
			else
			{
				normalElement.CalculateVertexNormals();
			}

			//Tangents
			if (hasTangents)
			{
				var tangentElement = new MeshSharp.Elements.Geometries.Layers.LayerElementTangent(outputMesh);
				outputMesh.Layers.Add(tangentElement);
				tangentElement.MappingInformationType = MappingMode.ByPolygonVertex;
				tangentElement.ReferenceInformationType = ReferenceMode.Direct;
				tangentElement.Tangents.AddRange(unityMesh.Tangents.Select(t => Convert(ToCoordinateSpace((Vector3f)t, ExportSpace))));
				//We're excluding W here because it's not supported by MeshSharp
				//For Unity, the tangent W coordinate denotes the direction of the binormal vector and is always 1 or -1
				//https://docs.unity3d.com/ScriptReference/Mesh-tangents.html
			}

			//UV
			if (hasUV0)
			{
				var uv0Element = new MeshSharp.Elements.Geometries.Layers.LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv0Element);
				uv0Element.MappingInformationType = MappingMode.ByPolygonVertex;
				uv0Element.ReferenceInformationType = ReferenceMode.IndexToDirect;
				uv0Element.UV.AddRange(unityMesh.UV0.Select(v => Convert(v)));
				uv0Element.UVIndex.AddRange(Enumerable.Range(0, unityMesh.UV0.Length));
			}
			if (hasUV1)
			{
				var uv1Element = new MeshSharp.Elements.Geometries.Layers.LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv1Element);
				uv1Element.MappingInformationType = MappingMode.ByPolygonVertex;
				uv1Element.ReferenceInformationType = ReferenceMode.IndexToDirect;
				uv1Element.UV.AddRange(unityMesh.UV1.Select(v => Convert(v)));
				uv1Element.UVIndex.AddRange(Enumerable.Range(0, unityMesh.UV1.Length));
			}

			//Colors
			if (hasColors)
			{
				var colorElement = new MeshSharp.Elements.Geometries.Layers.LayerElementVertexColor(outputMesh);
				outputMesh.Layers.Add(colorElement);
				colorElement.MappingInformationType = MappingMode.ByPolygonVertex;
				colorElement.ReferenceInformationType = ReferenceMode.IndexToDirect;
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

		private static Vector3f ToCoordinateSpace(Vector3f point, MeshCoordinateSpace space)
		{
			return space switch
			{
				MeshCoordinateSpace.Left => new Vector3f(-point.Y, point.Z, point.X),
				MeshCoordinateSpace.Right => new Vector3f(point.Z, -point.X, point.Y),
				MeshCoordinateSpace.Unity => point,
				_ => throw new ArgumentOutOfRangeException(nameof(space)),
			};
		}
	}
}
