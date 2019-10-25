using System.IO;

namespace uTinyRipper
{
	public interface IResourceFile
	{
		Stream Stream { get; }
		long Offset { get; }
		long Size { get; }
	}
}
