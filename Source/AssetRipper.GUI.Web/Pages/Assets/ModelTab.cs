using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class ModelTab : AssetHtmlTab
{
	public string Source { get; }

	public override string DisplayName => Localization.AssetTabModel;

	public override string HtmlName => "model";

	public override bool Enabled => AssetAPI.HasModelData(Asset);

	public ModelTab(IUnityObjectBase asset, AssetPath path) : base(asset)
	{
		Source = AssetAPI.GetModelUrl(path);
	}

	public override void Write(TextWriter writer)
	{
		using (new Table(writer).WithCustomAttribute("width", "100%").WithCustomAttribute("height", "100%").End())
		{
			using (new Tbody(writer).End())
			{
				using (new Tr(writer).End())
				{
					using (new Td(writer).WithAlign("center").WithCustomAttribute("valign", "middle").End())
					{
						new Canvas(writer)
							.WithId("babylonRenderCanvas")
							.WithStyle("width: 100%; height: 100vh;")
							.WithCustomAttribute("glb-data-path", Source)
							.Close();
					}
				}
			}
		}
	}
}
