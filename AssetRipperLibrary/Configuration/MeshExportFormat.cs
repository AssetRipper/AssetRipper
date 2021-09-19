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
		/// An opensource alternative to FBX. It is the binary version of GLTF. Primitive export only contains mesh data. Unity does not support importing this format.
		/// </summary>
		GlbPrimitive,
	}

	/* Unity supported mesh import formats: (https://docs.unity3d.com/2019.3/Documentation/Manual/3D-formats.html)
	 * .fbx
	 * .dae (Collada)
	 * .3ds
	 * .dxf
	 * .obj
	 */
}