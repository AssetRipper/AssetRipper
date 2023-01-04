namespace AssetRipper.Export.UnityProjects.Configuration
{
	public enum MeshExportFormat
	{
		/// <summary>
		/// A robust format for using meshes in the editor. Can be converted to other formats by a variety of unity packages.
		/// </summary>
		Native,
		/// <summary>
		/// An opensource alternative to FBX. It is the binary version of GLTF. Unity does not support importing this format.
		/// </summary>
		Glb,
	}
}
