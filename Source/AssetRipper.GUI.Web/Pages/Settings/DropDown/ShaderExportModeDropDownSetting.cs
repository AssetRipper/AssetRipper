using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class ShaderExportModeDropDownSetting : DropDownSetting<ShaderExportMode>
{
	public static ShaderExportModeDropDownSetting Instance { get; } = new();

	public override string Title => Localization.ShaderAssetExportTitle;

	protected override string GetDisplayName(ShaderExportMode value) => value switch
	{
		ShaderExportMode.Dummy => Localization.ShaderAssetFormatDummy,
		ShaderExportMode.Yaml => Localization.ShaderAssetFormatYaml,
		ShaderExportMode.Disassembly => Localization.ShaderAssetFormatDisassembly,
		ShaderExportMode.Decompile => Localization.ShaderAssetFormatDecompile,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(ShaderExportMode value) => value switch
	{
		ShaderExportMode.Dummy => Localization.ShaderAssetFormatDummyDescription,
		ShaderExportMode.Yaml => Localization.ShaderAssetFormatYamlDescription,
		ShaderExportMode.Disassembly => Localization.ShaderAssetFormatDisassemblyDescription,
		ShaderExportMode.Decompile => Localization.ShaderAssetFormatDecompileDescription,
		_ => base.GetDescription(value),
	};
}
