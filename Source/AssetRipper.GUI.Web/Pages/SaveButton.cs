namespace AssetRipper.GUI.Web.Pages;

internal static class SaveButton
{
	public static void Write(TextWriter writer, string url, string? fileName = null, string? text = null)
	{
		new A(writer).WithHref(url).WithDownload(fileName).WithClass("btn btn-primary").Close(text ?? Localization.Save);
	}
}
