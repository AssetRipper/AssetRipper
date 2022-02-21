using AssetRipper.Core.IO.Smart;
using System;
using System.IO;

namespace AssetRipper.Core.Parser.Files.ResourceFiles
{
	public sealed class ResourceFile : IResourceFile, IDisposable
	{
		internal ResourceFile(ResourceFileScheme scheme)
		{
			Name = scheme.NameOrigin;
			Stream = scheme.Stream.CreateReference();
		}

		~ResourceFile()
		{
			Dispose(false);
		}

		public static bool IsDefaultResourceFile(string fileName)
		{
			string extension = Path.GetExtension(fileName).ToLower();
			return extension switch
			{
				ResourceFileExtension or StreamingFileExtension => true,
				_ => false,
			};
		}

		public static ResourceFileScheme LoadScheme(string filePath, string fileName)
		{
			using SmartStream stream = SmartStream.OpenRead(filePath);
			return ReadScheme(stream, filePath, fileName);
		}

		public static ResourceFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			return ResourceFileScheme.ReadScheme(buffer, filePath, fileName);
		}

		public static ResourceFileScheme ReadScheme(SmartStream stream, string filePath, string fileName)
		{
			return ResourceFileScheme.ReadScheme(stream, filePath, fileName);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override string ToString()
		{
			return Name;
		}

		private void Dispose(bool _)
		{
			Stream.Dispose();
		}

		public string Name { get; }
		public Stream Stream { get; }

		public const string ResourceFileExtension = ".resource";
		public const string StreamingFileExtension = ".ress";
	}
}
