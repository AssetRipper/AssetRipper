using AssetRipper.Assets;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal abstract class AssetHtmlTab(IUnityObjectBase asset) : HtmlTab
{
	public IUnityObjectBase Asset { get; } = asset;
}
