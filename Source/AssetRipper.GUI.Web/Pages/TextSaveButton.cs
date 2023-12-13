using System.Text;

namespace AssetRipper.GUI.Web.Pages;

internal static class TextSaveButton
{
	public static void Write(TextWriter writer, string? fileName, string text)
	{
		string sourcePath = $"data:text/plain;base64,{Encoding.UTF8.GetBytes(text).ToBase64String()}";

		new A(writer).WithHref(sourcePath).WithDownload(fileName).WithClass("btn btn-primary").Close(Localization.Save);
	}
}
