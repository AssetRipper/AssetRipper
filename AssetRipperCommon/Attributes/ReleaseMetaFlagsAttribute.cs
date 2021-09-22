using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ReleaseMetaFlagsAttribute : Attribute
	{
		TransferMetaFlags Flags { get; }

		public ReleaseMetaFlagsAttribute(TransferMetaFlags flags)
		{
			Flags = flags;
		}
	}
}
