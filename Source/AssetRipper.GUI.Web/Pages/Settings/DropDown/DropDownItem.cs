namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public readonly record struct DropDownItem<T>(T Value, string DisplayName, string? Description);
