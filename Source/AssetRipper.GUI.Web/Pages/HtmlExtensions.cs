namespace AssetRipper.GUI.Web.Pages;

internal static class HtmlExtensions
{
	public static A WithNewTabAttributes(this A element)
	{
		return element.WithTarget("_blank").WithRel("noopener noreferrer");
	}

	public static Input MaybeWithChecked(this Input element, bool @checked)
	{
		return @checked ? element.WithChecked() : element;
	}

	public static Option MaybeSelected(this Option element, bool selected)
	{
		return selected ? element.WithSelected() : element;
	}

	public static Div WithTextCenter(this Div element)
	{
		return element.WithClass("text-center");
	}
}
