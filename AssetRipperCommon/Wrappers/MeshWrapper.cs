namespace AssetRipper.Core.Wrappers
{
	/// <summary>
	/// Wraps a mesh asset so that it can be accessed on any unity version
	/// </summary>
	public class MeshWrapper
	{
		private UnityObjectBase NativeMesh { get; }
		internal MeshWrapper(UnityObjectBase nativeMesh) => NativeMesh = nativeMesh;
		

	}
}
