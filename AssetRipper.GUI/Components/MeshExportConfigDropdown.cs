using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class MeshExportConfigDropdown : BaseConfigurationDropdown<MeshExportFormat>
	{
		protected override string GetValueDisplayName(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => MainWindow.Instance.LocalizationManager["mesh_format_native"],
			MeshExportFormat.Glb => MainWindow.Instance.LocalizationManager["mesh_format_glb_primitive"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => MainWindow.Instance.LocalizationManager["mesh_format_native_description"],
			MeshExportFormat.Glb => MainWindow.Instance.LocalizationManager["mesh_format_glb_primitive_description"],
			_ => null,
		};
	}
}
