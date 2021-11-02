using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Wrappers
{
	/// <summary>
	/// Wraps a mesh asset so that it can be accessed on any unity version
	/// </summary>
	public class MeshWrapper
	{
		private IUnityObjectBase NativeMesh { get; }
		internal MeshWrapper(IUnityObjectBase nativeMesh) => NativeMesh = nativeMesh;
		

	}
}
