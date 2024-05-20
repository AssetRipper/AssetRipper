using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Scenes;

public sealed class ViewPage : DefaultPage
{
	public required SceneDefinition Scene { get; init; }
	public required ScenePath Path { get; init; }

	public override string GetTitle() => Scene.Name;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());
		using (new Table(writer).WithClass("table").End())
		{
			using (new Tbody(writer).End())
			{
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.Name);
					new Td(writer).Close(Scene.Name);
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.Path);
					new Td(writer).Close(Scene.Path);
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.Guid);
					new Td(writer).Close(Scene.GUID.ToString());
				}
			}
		}
		new H2(writer).Close(Localization.Collections);
		using (new Ul(writer).End())
		{
			foreach (AssetCollection collection in Scene.Collections)
			{
				using (new Li(writer).End())
				{
					PathLinking.WriteLink(writer, collection);
				}
			}
		}
	}
}
