using System.Net;

namespace AssetRipper.GUI.Web.Paths;

internal static class StringExtensions
{
	[return: NotNullIfNotNull(nameof(value))]
	public static string? ToHtml(this string? value) => WebUtility.HtmlEncode(value);
	[return: NotNullIfNotNull(nameof(value))]
	public static string? ToUrl(this string? value) => WebUtility.UrlEncode(value);
}
