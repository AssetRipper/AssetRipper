using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface IBuildSettings : IUnityObjectBase
	{
		Utf8StringBase[] Scenes { get; }
		string Version { get; set; }
	}
}
