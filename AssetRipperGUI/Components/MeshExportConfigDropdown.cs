using AssetRipper.Library.Configuration;

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
			MeshExportFormat.PlyAscii => MainWindow.Instance.LocalizationManager["mesh_format_ply_ascii"],
			MeshExportFormat.GlbPrimitive => MainWindow.Instance.LocalizationManager["mesh_format_glb_primitive"],
			MeshExportFormat.FbxPrimitive => MainWindow.Instance.LocalizationManager["mesh_format_fbx_primitive"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => MainWindow.Instance.LocalizationManager["mesh_format_native_description"],
			MeshExportFormat.Obj => MainWindow.Instance.LocalizationManager["mesh_format_obj_description"],
			MeshExportFormat.StlAscii => MainWindow.Instance.LocalizationManager["mesh_format_stl_ascii_description"],
			MeshExportFormat.StlBinary => MainWindow.Instance.LocalizationManager["mesh_format_stl_binary_description"],
			MeshExportFormat.PlyAscii => MainWindow.Instance.LocalizationManager["mesh_format_ply_ascii_description"],
			MeshExportFormat.GlbPrimitive => MainWindow.Instance.LocalizationManager["mesh_format_glb_primitive_description"],
			MeshExportFormat.FbxPrimitive => MainWindow.Instance.LocalizationManager["mesh_format_fbx_primitive_description"],
			_ => null,
		};
	}
}