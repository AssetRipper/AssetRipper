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
				new P(writer).WithClass("mt-4").Close(Localization.TippingMessage);
				using (new Div(writer).WithClass("d-flex justify-content-center mt-3").End())
				{
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://assetripper.com/premium/get/paypal.html").Close("Paypal");
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://assetripper.com/premium/get/github-sponsors.html").Close("GitHub Sponsors");
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://assetripper.com/premium/get/buy-me-a-coffee.html").Close("Buy Me a Coffee");
					new A(writer).WithClass("btn btn-primary m-1").WithNewTabAttributes().WithHref("https://assetripper.com/premium/get/ko-fi.html").Close("Ko-fi");
				}
			}
		}
	}
}
