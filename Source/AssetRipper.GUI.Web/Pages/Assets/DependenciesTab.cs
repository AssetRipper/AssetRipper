using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class DependenciesTab(IUnityObjectBase asset) : AssetHtmlTab(asset)
{
	public override string DisplayName => Localization.AssetTabDependencies;
	public override string HtmlName => "dependencies";
	public override bool Enabled => Asset.FetchDependencies().Any(pair => !pair.Item2.IsNull);

	public override void Write(TextWriter writer)
	{
		using (new Table(writer).WithClass("table").End())
		{
			using (new Tbody(writer).End())
			{
				foreach ((string path, PPtr pptr) in Asset.FetchDependencies())
				{
					if (pptr.IsNull)
					{
						continue;
					}

					using (new Tr(writer).End())
					{
						new Th(writer).Close(path);
						using (new Td(writer).End())
						{
							IUnityObjectBase? dependency = Asset.Collection.TryGetAsset(pptr);
							if (dependency is null)
							{
								writer.WriteHtml("Missing");
							}
							else
							{
								PathLinking.WriteLink(writer, dependency);
							}
						}
					}
				}
			}
		}
	}
}
