using System.IO;

namespace AssetRipper.Parser.Files.ResourceFiles
{
	public interface IResourceFile
	{
		Stream Stream { get; }
	}
}
