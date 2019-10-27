namespace uTinyRipper.Converters
{
	public sealed class ExportOptions
	{
		/// <summary>
		/// Should objects get exported with dependencies or without
		/// </summary>
		public bool ExportDependencies { get; set; }

		public Version Version { get; set; }
		public Platform Platform { get; set; }
		public TransferInstructionFlags Flags { get; set; }
	}
}
