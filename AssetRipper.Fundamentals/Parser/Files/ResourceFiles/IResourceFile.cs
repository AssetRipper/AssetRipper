using System.IO;

namespace AssetRipper.Core.Parser.Files.ResourceFiles
{
	public interface IResourceFile
	{
		Stream Stream { get; }
	}
}
