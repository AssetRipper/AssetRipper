using AssetRipper.Core.Interfaces;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public interface IAssetBundle : IUnityObjectBase
	{
		string AssetBundleName { get; set; }
		KeyValuePair<string, IAssetInfo>[] GetAssets();
	}
}