using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class EditorMetaFlagsAttribute : Attribute
	{
		TransferMetaFlags Flags { get; }

		public EditorMetaFlagsAttribute(TransferMetaFlags flags)
		{
			Flags = flags;
		}
	}
}