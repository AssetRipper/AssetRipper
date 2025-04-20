using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages;

internal static class HtmlExtensions
{
	public static A WithNewTabAttributes(this A element)
	{
		return element.WithTarget("_blank").WithRel("noopener noreferrer");
	}

	public static Div WithTextCenter(this Div element)
	{
		return element.WithClass("text-center");
	}

	public static Pre WithDynamicTextContent(this Pre element, string url)
	{
		return element.WithCustomAttribute("dynamic-text-content", url.ToHtml());
	}
}
