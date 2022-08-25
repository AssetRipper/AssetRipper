using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Utils;
using System.Collections.Generic;

namespace AssetRipper.IO.Files.Schemes
{
	public abstract class FileScheme : IDisposable
	{
		protected FileScheme(string filePath, string fileName)
		{
			FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
			NameOrigin = fileName;
			Name = FilenameUtils.FixFileIdentifier(fileName);
		}

		public override string? ToString()
		{
			return Name ?? base.ToString();
		}

		public string FilePath { get; }
		public string NameOrigin { get; }
		public string Name { get; }

		public abstract IEnumerable<FileIdentifier> Dependencies { get; }

		~FileScheme()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing) { }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

	}
}
