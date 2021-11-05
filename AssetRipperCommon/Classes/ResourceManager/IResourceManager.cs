using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ResourceManager
{
	public interface IResourceManager : IUnityObjectBase
	{
		KeyValuePair<string, PPtr<IUnityObjectBase>>[] GetAssets();
	}
}