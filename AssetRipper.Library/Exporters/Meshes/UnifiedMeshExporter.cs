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
using System.Numerics;

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
			return IsSupported(ExportFormat) && asset is IMesh mesh && mesh.IsSet();
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
			using MemoryStream memoryStream = new();
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
			MeshSharp.Elements.Geometries.Mesh outputMesh = ConvertToMeshSharpMesh(unityMesh);
			Scene scene = new();
			scene.Name = unityMesh.NameString;
			Node node = new();
			node.Name = unityMesh.NameString;
			node.Children.Add(outputMesh);
			scene.Nodes.Add(node);
			return scene;
		}

		private MeshSharp.Elements.Geometries.Mesh ConvertToMeshSharpMesh(IMesh unityMesh)
		{
			unityMesh.ReadData(
				out Vector3[]? vertices,
				out Vector3[]? normals,
				out Vector4[]? tangents,
				out ColorFloat[]? colors,
				out _, //skin
				out Vector2[]? uv0,
				out Vector2[]? uv1,
				out Vector2[]? uv2,
				out Vector2[]? uv3,
				out Vector2[]? uv4,
				out Vector2[]? uv5,
				out Vector2[]? uv6,
				out Vector2[]? uv7,
				out _, //bindpose
				out uint[] processedIndexBuffer);

			if (vertices is null)
			{
				throw new ArgumentException("Vertices can't be null", nameof(unityMesh));
			}

			MeshSharp.Elements.Geometries.Mesh outputMesh = new();
			outputMesh.Name = unityMesh.NameString;
			
			//Vertices
			foreach (Vector3 vertex in vertices)
			{
				outputMesh.Vertices.Add(Convert(ToCoordinateSpace(vertex, ExportSpace)));
			}

			//Polygons
			List<uint> indices = TriangleProcessor.ReadIndices(unityMesh, processedIndexBuffer);
			if (indices.Count % 3 != 0)
			{
				throw new ArgumentException("Index count must be a multiple of 3.", nameof(unityMesh));
			}
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
			if (normals != null && normals.Length == vertices.Length)
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
			if (tangents != null && tangents.Length == vertices.Length)
			{
				LayerElementTangent tangentElement = new LayerElementTangent(outputMesh);
				outputMesh.Layers.Add(tangentElement);
				tangentElement.MappingInformationType = MappingMode.ByPolygonVertex;
				tangentElement.ReferenceInformationType = ReferenceMode.Direct;
				tangentElement.Tangents.AddRange(tangents.Select(t => Convert(ToCoordinateSpace(new Vector3(t.X, t.Y, t.Z), ExportSpace))));
				//We're excluding W here because it's not supported by MeshSharp
				//For Unity, the tangent W coordinate denotes the direction of the binormal vector and is always 1 or -1
				//https://docs.unity3d.com/ScriptReference/Mesh-tangents.html
			}

			//UV
			if (uv0 != null && uv0.Length == vertices.Length)
			{
				LayerElementUV uv0Element = new LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv0Element);
				uv0Element.MappingInformationType = MappingMode.ByPolygonVertex;
				uv0Element.ReferenceInformationType = ReferenceMode.IndexToDirect;
				uv0Element.UV.AddRange(uv0.Select(v => Convert(v)));
				uv0Element.UVIndex.AddRange(Enumerable.Range(0, uv0.Length));
			}
			if (uv1 != null && uv1.Length == vertices.Length)
			{
				LayerElementUV uv1Element = new LayerElementUV(outputMesh);
				outputMesh.Layers.Add(uv1Element);
				uv1Element.MappingInformationType = MappingMode.ByPolygonVertex;
				uv1Element.ReferenceInformationType = ReferenceMode.IndexToDirect;
				uv1Element.UV.AddRange(uv1.Select(v => Convert(v)));
				uv1Element.UVIndex.AddRange(Enumerable.Range(0, uv1.Length));
			}

			//Colors
			if (colors != null && colors.Length == vertices.Length)
			{
				LayerElementVertexColor colorElement = new LayerElementVertexColor(outputMesh);
				outputMesh.Layers.Add(colorElement);
				colorElement.MappingInformationType = MappingMode.ByPolygonVertex;
				colorElement.ReferenceInformationType = ReferenceMode.IndexToDirect;
				colorElement.Colors.AddRange(colors.Select(v => Convert(v)));
				colorElement.ColorIndex.AddRange(Enumerable.Range(0, colors.Length));
			}

			return outputMesh;
		}
		
		private static XY Convert(Vector2 vector) => new XY(vector.X, vector.Y);
		private static XYZ Convert(Vector3 vector) => new XYZ(vector.X, vector.Y, vector.Z);
		private static XYZM Convert(Vector4 vector) => new XYZM(vector.X, vector.Y, vector.Z, vector.W);
		private static XYZM Convert(ColorFloat vector) => new XYZM(vector.R, vector.G, vector.B, vector.A);
		
		private static Vector3 ToCoordinateSpace(Vector3 point, MeshCoordinateSpace space)
		{
			return space switch
			{
				MeshCoordinateSpace.Left => new Vector3(-point.Y, point.Z, point.X),
				MeshCoordinateSpace.Right => new Vector3(point.Z, -point.X, point.Y),
				MeshCoordinateSpace.Unity => point,
				_ => throw new ArgumentOutOfRangeException(nameof(space)),
			};
		}
	}
}
