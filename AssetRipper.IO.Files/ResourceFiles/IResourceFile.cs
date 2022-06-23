using System.IO;

namespace AssetRipper.IO.Files.ResourceFiles
{
	public interface IResourceFile
	{
		Stream Stream { get; }
	}
}
