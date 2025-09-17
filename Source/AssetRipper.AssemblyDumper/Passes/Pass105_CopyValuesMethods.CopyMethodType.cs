namespace AssetRipper.AssemblyDumper.Passes;

internal static partial class Pass105_CopyValuesMethods
{
	[Flags]
	private enum CopyMethodType
	{
		None = 0,
		Callvirt = 1,
		HasConverter = 2,
	}
}
