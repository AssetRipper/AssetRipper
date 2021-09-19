using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.Library.Exporters.Meshes
{
	public class StlMeshExporter : BaseMeshExporter
	{
		public StlMeshExporter(LibraryConfiguration configuration) : base(configuration)
		{
			BinaryExport = ExportFormat == MeshExportFormat.StlBinary;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Core.Classes.Object.Object asset)
		{
			return new AssetExportCollection(this, asset, "stl");
		}

		public override bool IsHandle(Mesh mesh)
		{
			return IsStlFormat(ExportFormat) && CanConvert(mesh);
		}

		public override byte[] ExportBinary(Mesh mesh)
		{
			return WriteBinary(mesh);
		}

		public override string ExportText(Mesh mesh)
		{
			return WriteString(mesh);
		}

		public static bool IsStlFormat(MeshExportFormat exportFormat)
		{
			return exportFormat == MeshExportFormat.StlAscii || exportFormat == MeshExportFormat.StlBinary;
		}

		public static bool CanConvert(Mesh mesh)
		{
			if (mesh.Vertices == null || mesh.Normals == null || mesh.Indices == null || mesh.Vertices.Length == 0 || mesh.Normals.Length == 0 || mesh.Indices.Count == 0)
				return false;
			if (mesh.BundleUnityVersion.IsLess(4))
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

		/// <summary>
		/// Write a mesh file to STL format.
		/// </summary>
		/// <param name="mesh">The mesh asset to write</param>
		/// <param name="type">How to format the file (in ASCII or binary)</param>
		public static byte[] WriteBinary(Mesh mesh, bool convertToRightHandedCoordinates = true)
		{
			return WriteBinary(new Mesh[] { mesh }, convertToRightHandedCoordinates);
		}

		/// <summary>
		/// Write a collection of mesh assets to an STL file
		/// </summary>
		/// <param name="meshes">The mesh assets to write</param>
		/// <param name="type">How to format the file (in ASCII or binary)</param>
		public static byte[] WriteBinary(IList<Mesh> meshes, bool convertToRightHandedCoordinates = true)
		{
			try
			{
				// http://paulbourke.net/dataformats/stl/
				// http://www.fabbers.com/tech/STL_Format
				using MemoryStream memoryStream = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(memoryStream, new ASCIIEncoding()))
				{
					// 80 byte header
					writer.Write(new byte[80]);

					uint totalTriangleCount = (uint)(meshes.Sum(x => x.Indices.Count) / 3);

					// unsigned long facet count (4 bytes)
					writer.Write(totalTriangleCount);

					foreach (Mesh mesh in meshes)
					{
						Vector3f[] v = mesh.Vertices;
						Vector3f[] n = mesh.Normals;

						if (convertToRightHandedCoordinates)
						{
							for (int i = 0, c = v.Length; i < c; i++)
							{
								v[i] = ToCoordinateSpace(v[i], CoordinateSpace.Right);
								n[i] = ToCoordinateSpace(n[i], CoordinateSpace.Right);
							}
						}

						uint[] t = mesh.Indices.ToArray();
						int triangleCount = t.Length;
						if (convertToRightHandedCoordinates)
							System.Array.Reverse(t);

						for (int i = 0; i < triangleCount; i += 3)
						{
							uint a = t[i], b = t[i + 1], c = t[i + 2];

							Vector3f avg = AvgNrm(n[a], n[b], n[c]);

							writer.Write(avg.X);
							writer.Write(avg.Y);
							writer.Write(avg.Z);

							writer.Write(v[a].X);
							writer.Write(v[a].Y);
							writer.Write(v[a].Z);

							writer.Write(v[b].X);
							writer.Write(v[b].Y);
							writer.Write(v[b].Z);

							writer.Write(v[c].X);
							writer.Write(v[c].Y);
							writer.Write(v[c].Z);

							// specification says attribute byte count should be set to 0.
							writer.Write((ushort)0);
						}
					}
				}
				return memoryStream.ToArray();
			}
			catch (System.Exception e)
			{
				Logger.Error(e);
				return null;
			}
		}

		/// <summary>
		/// Write a Unity mesh to an ASCII STL string.
		/// </summary>
		public static string WriteString(Mesh mesh, bool convertToRightHandedCoordinates = true)
		{
			return WriteString(new Mesh[] { mesh }, convertToRightHandedCoordinates);
		}

		/// <summary>
		/// Write a set of meshes to an ASCII string in STL format.
		/// </summary>
		public static string WriteString(IList<Mesh> meshes, bool convertToRightHandedCoordinates = true)
		{
			StringBuilder sb = new StringBuilder();

			string name = meshes.Count == 1 ? meshes[0].Name : "Composite Mesh";

			sb.AppendLine(string.Format("solid {0}", name));

			foreach (Mesh mesh in meshes)
			{
				Vector3f[] v = mesh.Vertices;
				Vector3f[] n = mesh.Normals;
				uint[] t = mesh.Indices.ToArray();

				if (convertToRightHandedCoordinates)
				{
					for (int i = 0, c = v.Length; i < c; i++)
					{
						v[i] = ToCoordinateSpace(v[i], CoordinateSpace.Right);
						n[i] = ToCoordinateSpace(n[i], CoordinateSpace.Right);
					}

					System.Array.Reverse(t);
				}

				int triLen = t.Length;

				for (int i = 0; i < triLen; i += 3)
				{
					uint a = t[i];
					uint b = t[i + 1];
					uint c = t[i + 2];

					Vector3f nrm = AvgNrm(n[a], n[b], n[c]);

					sb.AppendLine(string.Format("facet normal {0} {1} {2}", nrm.X, nrm.Y, nrm.Z));

					sb.AppendLine("outer loop");

					sb.AppendLine(string.Format("\tvertex {0} {1} {2}", v[a].X, v[a].Y, v[a].Z));
					sb.AppendLine(string.Format("\tvertex {0} {1} {2}", v[b].X, v[b].Y, v[b].Z));
					sb.AppendLine(string.Format("\tvertex {0} {1} {2}", v[c].X, v[c].Y, v[c].Z));

					sb.AppendLine("endloop");

					sb.AppendLine("endfacet");
				}
			}

			sb.AppendLine(string.Format("endsolid {0}", name));

			return sb.ToString();
		}

		/// <summary>
		/// Average of 3 vectors
		/// </summary>
		private static Vector3f AvgNrm(Vector3f a, Vector3f b, Vector3f c)
		{
			return new Vector3f(
				(a.X + b.X + c.X) / 3f,
				(a.Y + b.Y + c.Y) / 3f,
				(a.Z + b.Z + c.Z) / 3f);
		}

		private static Vector3f ToCoordinateSpace(Vector3f point, CoordinateSpace space)
		{
			if (space == CoordinateSpace.Left)
				return new Vector3f(-point.Y, point.Z, point.X);

			return new Vector3f(point.Z, -point.X, point.Y);
		}

		private enum CoordinateSpace
		{
			Left,
			Right
		}
	}
}
