using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files
{
	public interface IScheme
	{
		bool CanRead(SmartStream stream);
		File Read(SmartStream stream, string filePath, string fileName);
	}
}
