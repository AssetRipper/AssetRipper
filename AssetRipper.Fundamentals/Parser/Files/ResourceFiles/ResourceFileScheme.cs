using AssetRipper.Core.IO.Smart;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.Schemes;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Files.ResourceFiles
{
	public sealed class ResourceFileScheme : FileScheme
	{
		private ResourceFileScheme(SmartStream stream, string filePath, string fileName) : base(filePath, fileName)
		{
			Stream = stream.CreateReference();
		}

		internal static ResourceFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			using SmartStream stream = SmartStream.CreateMemory(buffer);
			return new ResourceFileScheme(stream, filePath, fileName);
		}

		internal static ResourceFileScheme ReadScheme(SmartStream stream, string filePath, string fileName)
		{
			return new ResourceFileScheme(stream, filePath, fileName);
		}

		public ResourceFile ReadFile()
		{
			return new ResourceFile(this);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Stream.Dispose();
		}

		public override FileEntryType SchemeType => FileEntryType.Resource;
		public override IEnumerable<FileIdentifier> Dependencies { get { yield break; } }

		public SmartStream Stream { get; }
	}
}
