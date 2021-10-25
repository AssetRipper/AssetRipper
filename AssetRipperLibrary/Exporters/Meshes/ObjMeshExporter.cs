using AssetRipper.Core;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using System.Collections.Generic;
using System.Text;

namespace AssetRipper.Library.Exporters.Meshes
{
	public class ObjMeshExporter : BaseMeshExporter
	{
		public ObjMeshExporter(LibraryConfiguration configuration) : base(configuration)
		{
			BinaryExport = false;
		}

		public override bool IsHandle(Mesh mesh)
		{
			return ExportFormat == MeshExportFormat.Obj && CanConvert(mesh);
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, "obj");
		}

		public override string ExportText(Mesh mesh)
		{
			return ConvertToObjString(mesh, true);
		}

		public static bool CanConvert(Mesh mesh)
		{
			if (mesh.Vertices == null || mesh.Normals == null || mesh.UV0 == null || mesh.Vertices.Length == 0 || mesh.Normals.Length == 0 || mesh.UV0.Length == 0)
				return false;
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


		private static int StartIndex { get; set; } = 0;
		private static void ResetIndex() => StartIndex = 0;
		public static string ConvertToObjString(Mesh mesh, bool makeSubmeshes)
		{
			ResetIndex();

			StringBuilder meshString = new StringBuilder();

			meshString.Append("#" + mesh.Name + ".obj"
							  + "\n#" + System.DateTime.Now.ToLongDateString()
							  + "\n#" + System.DateTime.Now.ToLongTimeString()
							  + "\n#-------"
							  + "\n\n");

			if (!makeSubmeshes)
			{
				meshString.Append("g ").Append(mesh.Name).Append("\n");
			}


			meshString.Append("#" + mesh.Name
							  + "\n#-------"
							  + "\n");
			if (makeSubmeshes)
			{
				meshString.Append("g ").Append(mesh.Name).Append("\n");
			}
			if (mesh == null)
				throw new System.ArgumentNullException(nameof(mesh));
			meshString.Append(MeshToString(ref mesh));


			ResetIndex();

			return meshString.ToString();
		}

		private static string MeshToString(ref Mesh mesh)
		{
			if (mesh == null)
				throw new System.ArgumentNullException(nameof(mesh));
			int numVertices = 0;

			StringBuilder sb = new StringBuilder();

			foreach (Vector3f vv in mesh.Vertices)
			{
				Vector3f v = vv;
				numVertices++;
				sb.Append(string.Format("v {0} {1} {2}\n", v.X, v.Y, -v.Z));
			}
			sb.Append("\n");
			foreach (Vector3f nn in mesh.Normals)
			{
				sb.Append(string.Format("vn {0} {1} {2}\n", -nn.X, -nn.Y, nn.Z));
			}
			sb.Append("\n");
			foreach (Vector2f v in mesh.UV0)
			{
				sb.Append(string.Format("vt {0} {1}\n", v.X, v.Y));
			}
			for (int material = 0; material < mesh.SubMeshes.Length; material++)
			{
				sb.Append("\n");
				sb.Append("usemtl ").Append($"Material{material}").Append("\n");
				sb.Append("usemap ").Append($"Material{material}").Append("\n");

				List<uint> triangles = mesh.Triangles[material];
				for (int i = 0; i < triangles.Count; i += 3)
				{
					sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
											triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
				}
			}

			StartIndex += numVertices;
			return sb.ToString();
		}
	}
}
