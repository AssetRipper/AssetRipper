using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages;

public sealed class IndexPage : DefaultPage
{
	public static IndexPage Instance { get; } = new();

	public override string? GetTitle() => GameFileLoader.Premium ? Localization.AssetRipperPremium : Localization.AssetRipperFree;

	public override void WriteInnerContent(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center container mt-5").End())
		{
			new H1(writer).WithClass("display-4 mb-4").Close(Localization.Welcome);
			if (GameFileLoader.IsLoaded)
			{
				PathLinking.WriteLink(writer, GameFileLoader.GameBundle, Localization.ViewLoadedFiles, "btn btn-success");
			}
			else
			{
				new Button(writer).WithType("button").WithClass("btn btn-secondary").WithDisabled().Close(Localization.NoFilesLoaded);
			}
			if (GameFileLoader.Premium)
			{
				new P(writer).WithClass("mt-4").Close(Localization.AppreciationMessage);
			}
			else
			{
				new P(writer).WithClass("mt-4").Close(Localization.DonationMessage);
				using (new Div(writer).WithClass("d-flex justify-content-center mt-3").End())
				{
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://discord.gg/2QHekWpMYE").Close("Steven Unity Hub");
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://creeper1212.github.io/Gorilla-Tag-Version-History-Explorer/").Close("Gorilla Tag Version History Explorer");
				}
			}
		}
	}
}
