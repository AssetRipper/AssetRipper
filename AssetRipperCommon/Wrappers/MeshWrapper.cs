namespace AssetRipper.Core.Wrappers
{
	/// <summary>
	/// Wraps a mesh asset so that it can be accessed on any unity version
	/// </summary>
	public class MeshWrapper
	{
		private object NativeMesh { get; }
		internal MeshWrapper(object nativeMesh) => NativeMesh = nativeMesh;
		

	}
}
