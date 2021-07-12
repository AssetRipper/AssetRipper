using System.IO;

namespace AssetRipper
{
	public interface IResourceFile
	{
		Stream Stream { get; }
	}
}
