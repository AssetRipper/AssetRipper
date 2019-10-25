using uTinyRipper.Classes;
using uTinyRipper.Classes.Meshes;

namespace uTinyRipper.Converters.Meshes
{
	public static class TangentConverter
	{
		public static Vector3f[] GenerateNormals(Tangent[] origin)
		{
			Vector3f[] normals = new Vector3f[origin.Length];
			for (int i = 0; i < origin.Length; i++)
			{
				normals[i] = origin[i].Normal;
			}
			return normals;
		}

		public static Vector4f[] GenerateTangents(Tangent[] origin)
		{
			Vector4f[] tangents = new Vector4f[origin.Length];
			for (int i = 0; i < origin.Length; i++)
			{
				Vector4f tangent = (Vector4f)origin[i].TangentValue;
				tangent.W = origin[i].Handedness;
				tangents[i] = tangent;
			}
			return tangents;
		}
	}
}
