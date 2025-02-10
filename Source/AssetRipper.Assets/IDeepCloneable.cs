using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets;

public interface IDeepCloneable
{
	/// <summary>
	/// Deep clones this object.
	/// </summary>
	/// <param name="converter">The converter to use for cloning <see cref="PPtr"/>s.</param>
	/// <returns>The cloned object.</returns>
	IUnityAssetBase DeepClone(PPtrConverter converter);
}
