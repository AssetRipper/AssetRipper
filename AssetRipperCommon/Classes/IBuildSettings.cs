using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes
{
	public interface IBuildSettings : IAsset
	{
		string[] Scenes { get; set; }
		string Version { get; set; }
	}
}