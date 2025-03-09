using AssetRipper.Assets.Bundles;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files;

namespace AssetRipper.GUI.Web.Pages.FailedFiles;

public sealed class ViewPage : DefaultPage
{
	public required FailedFile File { get; init; }
	public required FailedFilePath Path { get; init; }

	public Bundle? Bundle => GameFileLoader.GameBundle.TryGetBundle(Path.BundlePath);

	public override string GetTitle() => File.NameFixed;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		if (Bundle is { } bundle)
		{
			new H2(writer).Close(Localization.Bundle);
			PathLinking.WriteLink(writer, Path.BundlePath, bundle.Name);
		}

		new H2(writer).Close(Localization.StackTrace);

		string url = FailedFileAPI.GetStackTraceUrl(Path);

		new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").WithDynamicTextContent(url).Close();

		using (new Div(writer).WithClass("text-center").End())
		{
			SaveButton.Write(writer, url, "error.txt");
		}
	}
}
