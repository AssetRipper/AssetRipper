namespace AssetRipper.GUI.Web.Pages.Assets;

internal abstract class AssetTab
{
	public abstract string DisplayName { get; }
	public abstract string HtmlName { get; }
	public abstract bool Enabled { get; }
	public abstract void Write(TextWriter writer);
}
