namespace AssetRipper.GUI.Web.Pages;

internal static class DataSaveButton
{
	public static void Write(TextWriter writer, string? fileName, byte[] data)
	{
		string sourcePath = $"data:application/octet-stream;base64,{data.ToBase64String()}";

		new A(writer).WithHref(sourcePath).WithDownload(fileName).WithClass("btn btn-primary").Close(Localization.Save);
	}
}
