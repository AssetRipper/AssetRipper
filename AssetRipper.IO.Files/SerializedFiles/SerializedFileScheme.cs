using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Converters;
using AssetRipper.IO.Files.Extensions;
using AssetRipper.IO.Files.Schemes;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.Smart;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.IO.Files.SerializedFiles
{
	public sealed class SerializedFileScheme : Scheme<SerializedFile>
	{
		public override bool CanRead(SmartStream stream)
		{
			return SerializedFile.IsSerializedFile(stream);
		}
	}
}
