using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public interface IAssetBundle : IUnityObjectBase
	{
		string AssetBundleName { get; set; }
		NullableKeyValuePair<string, IAssetInfo>[] GetAssets();
	}
}