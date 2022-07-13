using AssetRipper.Core;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using MeshSharp;
using MeshSharp.Elements;
using MeshSharp.Elements.Geometries.Layers;
using MeshSharp.FBX;
using MeshSharp.OBJ;
using MeshSharp.PLY;
using MeshSharp.STL;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetRipper.Library.Exporters.Meshes
{
	public sealed class UnifiedMeshExporter : BinaryAssetExporter
	{
		public MeshExportFormat ExportFormat { get; }
		public MeshCoordinateSpace ExportSpace { get; }
		public UnifiedMeshExporter(LibraryConfiguration configuration)
		{
			ExportFormat = configuration.MeshExportFormat;
			ExportSpace = configuration.MeshCoordinateSpace;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new UnifiedMeshExportCollection(this, asset);
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			if (asset is IMesh mesh)
			{
				return IsSupported(ExportFormat) && HasValidMeshData(mesh);
			}
			else
			{
				return false;
			}
		}

		public static bool HasValidMeshData(IMesh mesh)
		{
			return mesh != null &&
				
				//mesh.Vertices != null &&
				//mesh.Vertices.Length > 0 &&
				//mesh.Indices != null &&
				//mesh.Indices.Count > 0 &&
				//mesh.Indices.Count % 3 == 0 &&
				IsNotLinesOrPoints(mesh);
		}

		private static bool IsNotLinesOrPoints(IMesh mesh)
		{
			//minor optimization since submesh topology couldn't be lines or points before Unity 4
			if (mesh.SerializedFile.Version.IsLess(4))
			{
				return true;
			}

			foreach (ISubMesh submesh in mesh.SubMeshes_C43)
			{
				switch (submesh.GetTopology())
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
			byte[] data = ExportBinary((IMesh)asset);
			if (data == null || data.Length == 0)
			{
				return false;
			}

			TaskManager.AddTask(File.WriteAllBytesAsync(path, data));
			return true;
		}

		private byte[] ExportBinary(IMesh mesh)
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

		private Scene ConvertToScene(IMesh unityMesh)
		{
			MeshSharp.Elements.Geometries.Mesh? outputMesh = ConvertToMeshSharpMesh(unityMesh);
			Scene scene = new Scene();
			scene.Name = unityMesh.NameString;
			Node node = new Node();
			node.Name = unityMesh.NameString;
			node.Children.Add(outputMesh);
			scene.Nodes.Add(node);
			return scene;
		}

		private MeshSharp.Elements.Geometries.Mesh ConvertToMeshSharpMesh(IMesh unityMesh)
		{
			unityMesh.ReadData(
				out Vector3f[]? vertices,
				out Vector3f[]? normals,
				out Vector4f[]? tangents,
				out ColorRGBA32[]? colors,
				out _, //skin
				out Vector2f[]? uv0,
				out Vector2f[]? uv1,
				out Vector2f[]? uv2,
				out Vector2f[]? uv3,
				out Vector2f[]? uv4,
				out Vector2f[]? uv5,
				out Vector2f[]? uv6,
				out Vector2f[]? uv7,
				out _, //bindpose
				out uint[] processedIndexBuffer);

			if (vertices is null)
			{
				throw new ArgumentException("Vertices can't be null", nameof(unityMesh));
			}

			bool hasVertexNormals = normals != null && normals.Length == vertices.Length;
			bool hasTangents = tangents != null && tangents.Length == vertices.Length;
			bool hasUV0 = uv0 != null && uv0.Length == vertices.Length;
			bool hasUV1 = uv1 != null && uv1.Length == vertices.Length;
			bool hasColors = colors != null && colors.Length == vertices.Length;
			//Logger.Info($"VertexNormals: {hasVertexNormals}");
			//Logger.Info($"Tangents: {hasTangents}");
			//Logger.Info($"UV 0: {hasUV0}");
			//Logger.Info($"UV 1: {hasUV1}");
			//Logger.Info($"Colors: {hasColors}");
			MeshSharp.Elements.Geometries.Mesh? outputMesh = new MeshSharp.Elements.Geometries.Mesh();
			outputMesh.Name = unityMesh.NameString;

			//Vertices
			foreach (Vector3f vertex in vertices)
			{
				outputMesh.Vertices.Add(Convert(ToCoordinateSpace(vertex, ExportSpace)));
			}

			//Polygons
			List<uint> indices = TriangleProcessor.ReadIndices(unityMesh, processedIndexBuffer);
			for (int i = 0; i + 2 < indices.Count; i += 3)
			{
				if (ExportSpace == MeshCoordinateSpace.Right) //Switching to a right handed coordinate system requires reversing the polygon vertex order
				{
					outputMesh.Polygons.Add(new Triangle(indices[i + 2], indices[i + 1], indices[i]));
				}
				else
				{
					outputMesh.Polygons.Add(new Triangle(indices[i], indices[i + 1], indices[i + 2]));
				}
			}

			//Normals
			LayerElementNormal normalElement = new LayerElementNormal(outputMesh);
			outputMesh.Layers.Add(normalElement);
			if (hasVertexNormals)
			{
				normalElement.MappingInformationType = MappingMode.ByPolygonVertex;
				normalElement.ReferenceInformationType = ReferenceMode.Direct;
				foreach (Polygon? polygon in outputMesh.Polygons)
				{
					foreach (uint index in polygon.Indices)
					{
						normalElement.Normals.Add(Convert(ToCoordinateSpace(normals[index], ExportSpace)));
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
				LayerElementTangent tangentElement = new LayerElementTangent(outputMesh);
				outputMesh.Layers.Add(tangentElement);
				tangentElement.MappingInformationType = MappingMode.ByPolygonVertex;
				tangentElement.ReferenceInformationType = ReferenceMode.Direct;
				tangentElement.Tangents.AddRange(tangents.Select(t => Convert(ToCoordinateSpace((Vector3f)t, ExportSpace))));
				//We're excluding W here because it's not supported by MeshSharp
				//For Unity, the tangent W coordinate denotes the direction of the binormal vector and is always 1 or -1
				//https://docs.unity3d.com/ScriptReference/Mesh-tangents.html
			}

			//UV
			if (hasUV0)
			{
				LayerElementUV? uv0Element = new LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv0Element);
				uv0Element.MappingInformationType = MappingMode.ByPolygonVertex;
				uv0Element.ReferenceInformationType = ReferenceMode.IndexToDirect;
				uv0Element.UV.AddRange(uv0.Select(v => Convert(v)));
				uv0Element.UVIndex.AddRange(Enumerable.Range(0, uv0.Length));
			}
			if (hasUV1)
			{
				LayerElementUV? uv1Element = new LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv1Element);
				uv1Element.MappingInformationType = MappingMode.ByPolygonVertex;
				uv1Element.ReferenceInformationType = ReferenceMode.IndexToDirect;
				uv1Element.UV.AddRange(uv1.Select(v => Convert(v)));
				uv1Element.UVIndex.AddRange(Enumerable.Range(0, uv1.Length));
			}

			//Colors
			if (hasColors)
			{
				LayerElementVertexColor? colorElement = new LayerElementVertexColor(outputMesh);
				outputMesh.Layers.Add(colorElement);
				colorElement.MappingInformationType = MappingMode.ByPolygonVertex;
				colorElement.ReferenceInformationType = ReferenceMode.IndexToDirect;
				colorElement.Colors.AddRange(colors.Select(v => Convert(v)));
				colorElement.ColorIndex.AddRange(Enumerable.Range(0, colors.Length));
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
