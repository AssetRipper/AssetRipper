using ElectronNET.API;

namespace AssetRipper.GUI.Electron;

public static class LocalHost
{
	/// <summary>
	/// The base url for the web server, e.g. http://localhost:8001/
	/// </summary>
	public static string BaseUrl { get; private set; } = "";

	public static void Initialize()
	{
		BaseUrl = $"http://localhost:{BridgeSettings.WebPort}/";
	}
}
