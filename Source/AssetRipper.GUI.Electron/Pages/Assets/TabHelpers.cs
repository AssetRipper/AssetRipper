namespace AssetRipper.GUI.Electron.Pages.Assets;

internal static class TabHelpers
{
	public static string GetTabClassSet(bool disabled)
	{
		const string DefaultTabClassSet = "nav-link";
		return DefaultTabClassSet + (disabled ? " disabled" : "");
	}
}
