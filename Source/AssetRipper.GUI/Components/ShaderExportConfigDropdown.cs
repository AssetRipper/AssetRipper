using AssetRipper.Export.UnityProjects.Configuration;

namespace AssetRipper.GUI.Components
{
	public class ShaderExportConfigDropdown : BaseConfigurationDropdown<ShaderExportMode>
	{
		protected override string GetValueDisplayName(ShaderExportMode value) => value switch
		{
			ShaderExportMode.Dummy => MainWindow.Instance.LocalizationManager["shader_asset_format_dummy"],
			ShaderExportMode.Yaml => MainWindow.Instance.LocalizationManager["shader_asset_format_yaml"],
			ShaderExportMode.Disassembly => MainWindow.Instance.LocalizationManager["shader_asset_format_disassembly"],
			ShaderExportMode.Decompile => MainWindow.Instance.LocalizationManager["shader_asset_format_decompile"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(ShaderExportMode value) => value switch
		{
			ShaderExportMode.Dummy => MainWindow.Instance.LocalizationManager["shader_asset_format_dummy_description"],
			ShaderExportMode.Yaml => MainWindow.Instance.LocalizationManager["shader_asset_format_yaml_description"],
			ShaderExportMode.Disassembly => MainWindow.Instance.LocalizationManager["shader_asset_format_disassembly_description"],
			ShaderExportMode.Decompile => MainWindow.Instance.LocalizationManager["shader_asset_format_decompile_description"],
			_ => null,
		};
	}
}
