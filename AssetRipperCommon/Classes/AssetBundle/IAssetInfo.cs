using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public interface IAssetInfo
	{
		PPtr<IUnityObjectBase> AssetPtr { get; }
	}
}