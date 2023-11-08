namespace AssetRipper.GUI.Electron;

/// <summary>
/// <see href="https://getbootstrap.com/" />
/// </summary>
internal static class BootstrapInformation
{
	public const string Version = "5.3.2";

	public static class StyleSheet
	{
		public const string Url = $"https://cdn.jsdelivr.net/npm/bootstrap@{Version}/dist/css/bootstrap.min.css";
		public const string Integrity = "sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN";
	}

	public static class Script
	{
		public const string Url = $"https://cdn.jsdelivr.net/npm/bootstrap@{Version}/dist/js/bootstrap.bundle.min.js";
		public const string Integrity = "sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL";
	}
}
