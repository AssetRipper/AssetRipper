using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface IBuildSettings : IUnityObjectBase
	{
		string[] Scenes { get; set; }
		string Version { get; set; }
	}
}