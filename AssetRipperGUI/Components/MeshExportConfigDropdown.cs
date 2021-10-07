using AssetRipper.Library.Configuration;
using System;

namespace AssetRipper.GUI.Components
{
	public class MeshExportConfigDropdown : BaseConfigurationDropdown<MeshExportFormat>
	{
		protected override string GetValueDisplayName(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => MainWindow.Instance.LocalizationManager["mesh_format_native"],
			MeshExportFormat.Obj => MainWindow.Instance.LocalizationManager["mesh_format_obj"],
			MeshExportFormat.StlAscii => MainWindow.Instance.LocalizationManager["mesh_format_stl_ascii"],
			MeshExportFormat.StlBinary => MainWindow.Instance.LocalizationManager["mesh_format_stl_binary"],
			MeshExportFormat.GlbPrimitive => MainWindow.Instance.LocalizationManager["mesh_format_glb_primitive"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => MainWindow.Instance.LocalizationManager["mesh_format_native_description"],
			MeshExportFormat.Obj => MainWindow.Instance.LocalizationManager["mesh_format_obj_description"],
			MeshExportFormat.StlAscii => MainWindow.Instance.LocalizationManager["mesh_format_stl_ascii_description"],
			MeshExportFormat.StlBinary => MainWindow.Instance.LocalizationManager["mesh_format_stl_binary_description"],
			MeshExportFormat.GlbPrimitive => MainWindow.Instance.LocalizationManager["mesh_format_glb_primitive_description"],
			MeshExportFormat.FbxPrimitive => "Don't use. Not fully implemented",
			_ => null,
		};
	}
}