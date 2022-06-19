using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Parser.Utils;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Files.Schemes
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
			return Name == null ? base.ToString() : $"T:{SchemeType} N:'{Name}'";
		}

		public string FilePath { get; }
		public string NameOrigin { get; }
		public string Name { get; }

		public abstract FileEntryType SchemeType { get; }
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
