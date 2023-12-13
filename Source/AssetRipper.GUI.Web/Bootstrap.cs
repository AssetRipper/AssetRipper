namespace AssetRipper.GUI.Web;

internal static class Bootstrap
{
	internal static void WriteStyleSheetReference(TextWriter writer)
	{
		new Link(writer)
		{
			Rel = "stylesheet",
			Href = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css",
			Integrity = "sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN",
			Crossorigin = "anonymous"
		}.Close();
	}

	internal static void WriteScriptReference(TextWriter writer)
	{
		new Script(writer)
		{
			Src = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js",
			Integrity = "sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL",
			Crossorigin = "anonymous"
		}.Close();
	}
}
