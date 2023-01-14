using AssetRipper.Export.UnityProjects.Configuration;

namespace AssetRipper.GUI.Components
{
	public class MeshExportConfigDropdown : BaseConfigurationDropdown<MeshExportFormat>
	{
		protected override string GetValueDisplayName(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => MainWindow.Instance.LocalizationManager["mesh_format_native"],
			MeshExportFormat.Glb => MainWindow.Instance.LocalizationManager["mesh_format_glb"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => MainWindow.Instance.LocalizationManager["mesh_format_native_description"],
			MeshExportFormat.Glb => MainWindow.Instance.LocalizationManager["mesh_format_glb_description"],
			_ => null,
		};
	}
}
