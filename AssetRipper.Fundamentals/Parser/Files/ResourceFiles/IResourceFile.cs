using AssetRipper.Core.IO.Smart;

namespace AssetRipper.Core.Parser.Files.ResourceFiles
{
	public interface IResourceFile
	{
		SmartStream Stream { get; }
	}
}
