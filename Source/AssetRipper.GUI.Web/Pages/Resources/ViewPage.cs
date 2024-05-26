using AssetRipper.Assets.Bundles;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.GUI.Web.Pages.Resources;

public sealed class ViewPage : DefaultPage
{
	public required ResourceFile Resource { get; init; }
	public required ResourcePath Path { get; init; }

	public Bundle? Bundle => GameFileLoader.GameBundle.TryGetBundle(Path.BundlePath);

	public override string GetTitle() => Resource.NameFixed;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		if (Bundle is { } bundle)
		{
			new H2(writer).Close(Localization.Bundle);
			PathLinking.WriteLink(writer, Path.BundlePath, bundle.Name);
		}

		new H2(writer).Close(Localization.Size);
		new P(writer).Close(Resource.Stream.Length.ToString());

		using (new Div(writer).WithClass("text-center").End())
		{
			SaveButton.Write(writer, ResourceAPI.GetDataUrl(Path), Resource.NameFixed);
		}
	}
}
