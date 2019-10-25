namespace uTinyRipper.Converters
{
	public sealed class ExportOptions
	{
		public Version Version { get; set; }
		public Platform Platform { get; set; }
		public TransferInstructionFlags Flags { get; set; }
	}
}
