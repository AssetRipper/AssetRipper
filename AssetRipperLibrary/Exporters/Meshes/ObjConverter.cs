using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Math;
using System.Text;

namespace AssetRipper.Library.Exporters.Meshes
{
	public static class ObjConverter
	{
		public static bool CanConvert(Mesh mesh)
		{
			if (mesh.BundleUnityVersion.IsLess(4)) return true;
			foreach(var submesh in mesh.SubMeshes)
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
		public static void ResetIndex() => StartIndex = 0;
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
				Vector3f v = nn;
				sb.Append(string.Format("vn {0} {1} {2}\n", -v.X, -v.Y, v.Z));
			}
			sb.Append("\n");
			foreach (Vector3f v in mesh.UV0)
			{
				sb.Append(string.Format("vt {0} {1}\n", v.X, v.Y));
			}
			for (int material = 0; material < mesh.SubMeshes.Length; material++)
			{
				sb.Append("\n");
				sb.Append("usemtl ").Append($"Material{material}").Append("\n");
				sb.Append("usemap ").Append($"Material{material}").Append("\n");

				uint[] triangles = null; // mesh.GetTriangles(material);
				for (int i = 0; i < triangles.Length; i += 3)
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
