using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;

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
