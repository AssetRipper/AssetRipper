namespace AssetRipper.Library.Configuration
{
	public enum MeshExportFormat
	{
		/// <summary>
		/// A robust format for using meshes in the editor. Can be converted to other formats by a variety of unity packages.
		/// </summary>
		Native,
		/// <summary>
		/// A common text format usuable in a variety of applications. However, this also breaks exported references to the mesh asset.
		/// </summary>
		Obj,
		/// <summary>
		/// A common text format used in the 3D printing industry. Unity does not support importing this format.
		/// </summary>
		StlAscii,
		/// <summary>
		/// A common binary format used in the 3D printing industry. Unity does not support importing this format.
		/// </summary>
		StlBinary,
		/// <summary>
		/// Stanford Triangle Format for storing mesh data as text. Unity does not support importing this format.
		/// </summary>
		PlyAscii,
		/// <summary>
		/// An opensource alternative to FBX. It is the binary version of GLTF. Primitive export only contains mesh data. Unity does not support importing this format.
		/// </summary>
		GlbPrimitive,
		/// <summary>
		/// FBX. Primitive export only contains mesh data. This option breaks exported references to the mesh asset.
		/// </summary>
		FbxPrimitive,
	}

	/* Unity supported mesh import formats: (https://docs.unity3d.com/2019.3/Documentation/Manual/3D-formats.html)
	 * .fbx
	 * .dae (Collada)
	 * .3ds
	 * .dxf
	 * .obj
	 */

	public static class MeshExportFormatExtensions
	{
		public static string GetFileExtension(this MeshExportFormat format) => format switch
		{
			MeshExportFormat.Native => "asset",
			MeshExportFormat.Obj => "obj",
			MeshExportFormat.StlAscii => "stl",
			MeshExportFormat.StlBinary => "stl",
			MeshExportFormat.PlyAscii => "ply",
			MeshExportFormat.GlbPrimitive => "glb",
			MeshExportFormat.FbxPrimitive => "fbx",
			_ => null,
		};

		public static bool IsFBX(this MeshExportFormat format) => format == MeshExportFormat.FbxPrimitive;
		public static bool IsGLB(this MeshExportFormat format) => format == MeshExportFormat.GlbPrimitive;
		public static bool IsOBJ(this MeshExportFormat format) => format == MeshExportFormat.Obj;
		public static bool IsPLY(this MeshExportFormat format) => format == MeshExportFormat.PlyAscii;
		public static bool IsSTL(this MeshExportFormat format) => format == MeshExportFormat.StlAscii || format == MeshExportFormat.StlBinary;
		public static bool IsYaml(this MeshExportFormat format) => format == MeshExportFormat.Native;
	}
}