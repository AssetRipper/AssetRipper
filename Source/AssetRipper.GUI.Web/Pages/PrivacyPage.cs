namespace AssetRipper.GUI.Web.Pages;

public sealed class PrivacyPage : DefaultPage
{
	public static PrivacyPage Instance { get; } = new();

	public override string GetTitle() => "Privacy Policy";

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());
		using (new P(writer).End())
		{
			writer.Write("This app does not access the internet.");
		}
	}
}
