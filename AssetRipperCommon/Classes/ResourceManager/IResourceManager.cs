using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;

namespace AssetRipper.Core.Classes.ResourceManager
{
	public interface IResourceManager : IUnityObjectBase
	{
		NullableKeyValuePair<Utf8StringBase, PPtr<IUnityObjectBase>>[] GetAssets();
	}
}
