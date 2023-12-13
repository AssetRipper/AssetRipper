namespace AssetRipper.GUI.Web.Pages;

internal static class ByteArrayExtensions
{
	public static string ToBase64String(this byte[] array)
	{
		return Convert.ToBase64String(array, Base64FormattingOptions.None);
	}
}
