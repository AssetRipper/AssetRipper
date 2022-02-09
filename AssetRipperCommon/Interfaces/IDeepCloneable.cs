namespace AssetRipper.Core.Interfaces
{
	public interface IDeepCloneable
	{
		/// <summary>
		/// Create a deep clone of this object
		/// </summary>
		/// <returns>The new object instance</returns>
		UnityAssetBase DeepClone();
	}
}
