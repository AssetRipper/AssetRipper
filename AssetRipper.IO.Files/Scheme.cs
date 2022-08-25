using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files
{
	public abstract class Scheme<T> : IScheme where T : File, new()
	{
		public abstract bool CanRead(SmartStream stream);

		public T Read(SmartStream stream, string filePath, string fileName)
		{
			T file = new();
			file.FilePath = filePath;
			file.Name = fileName;
			file.Read(stream);
			return file;
		}

		File IScheme.Read(SmartStream stream, string filePath, string fileName) => Read(stream, filePath, fileName);
	}
}
