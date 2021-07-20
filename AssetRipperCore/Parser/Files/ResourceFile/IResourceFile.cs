using System.IO;

namespace AssetRipper.Parser.Files.ResourceFile
{
	public interface IResourceFile
	{
		Stream Stream { get; }
	}
}
