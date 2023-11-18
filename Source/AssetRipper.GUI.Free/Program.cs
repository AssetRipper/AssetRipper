namespace AssetRipper.GUI.Free;

internal static class Program
{
	static async Task Main(string[] args)
	{
		await Electron.Program.Run(args);
	}
}
