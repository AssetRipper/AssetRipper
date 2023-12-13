using AssetRipper.Export.UnityProjects.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class MeshExportFormatDropDownSetting : DropDownSetting<MeshExportFormat>
{
	public static MeshExportFormatDropDownSetting Instance { get; } = new();

	public override string Title => Localization.MeshExportTitle;

	protected override string GetDisplayName(MeshExportFormat value) => value switch
	{
		MeshExportFormat.Native => Localization.MeshFormatNative,
		MeshExportFormat.Glb => Localization.MeshFormatGlb,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(MeshExportFormat value) => value switch
	{
		MeshExportFormat.Native => Localization.MeshFormatNativeDescription,
		MeshExportFormat.Glb => Localization.MeshFormatGlbDescription,
		_ => base.GetDescription(value),
	};
}
