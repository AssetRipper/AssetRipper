using AssetRipper.Import.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class StreamingAssetsModeDropDownSetting : DropDownSetting<StreamingAssetsMode>
{
	public static StreamingAssetsModeDropDownSetting Instance { get; } = new();

	public override string Title => nameof(StreamingAssetsMode);

	protected override string? GetDescription(StreamingAssetsMode value)
	{
		return null;
	}
}
