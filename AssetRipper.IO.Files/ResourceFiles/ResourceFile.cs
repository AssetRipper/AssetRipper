using AssetRipper.IO.Files.Streams.Smart;
using System.IO;

namespace AssetRipper.IO.Files.ResourceFiles
{
	public sealed class ResourceFile : File
	{
		internal ResourceFile(SmartStream stream, string filePath, string name)
		{
			Stream = stream.CreateReference();
			FilePath = filePath;
			Name = name;
		}

		public bool IsDefaultResourceFile() => IsDefaultResourceFile(Name);

		public static bool IsDefaultResourceFile(string fileName)
		{
			string extension = Path.GetExtension(fileName).ToLowerInvariant();
			return extension is ResourceFileExtension or StreamingFileExtension;
		}

		public override string ToString() => Name;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Stream.Dispose();
		}

		public override void Read(SmartStream stream)
		{
			throw new NotSupportedException();
		}

		public override void Write(Stream stream)
		{
			Stream.CopyTo(stream);
		}

		public SmartStream Stream { get; }

		public const string ResourceFileExtension = ".resource";
		public const string StreamingFileExtension = ".ress";
	}
}
