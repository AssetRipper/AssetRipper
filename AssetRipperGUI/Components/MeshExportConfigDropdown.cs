using AssetRipper.Library.Configuration;
using System;

namespace AssetRipper.GUI.Components
{
	public class MeshExportConfigDropdown : BaseConfigurationDropdown<MeshExportFormat>
	{
		protected override string GetValueDisplayName(MeshExportFormat value) => value switch
		{
			MeshExportFormat.GlbPrimitive => "GLB Primitive",
			MeshExportFormat.StlAscii => "STL (Ascii)",
			MeshExportFormat.StlBinary => "STL (Binary)",
			MeshExportFormat.Obj => "OBJ",
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(MeshExportFormat value) => value switch
		{
			MeshExportFormat.Native => "A robust format for using meshes in the editor. Can be converted to other formats by a variety of unity packages.",
			MeshExportFormat.Obj => "Very widely-used text-base format, usable in almost all 3d editing software. However, this breaks exported references to the mesh asset.",
			MeshExportFormat.StlAscii => "3D object format commonly used for 3D printing. Unity cannot import assets of this type. Text-based variant.",
			MeshExportFormat.StlBinary => "3D object format commonly used for 3D printing. Unity cannot import assets of this type. Binary variant.",
			MeshExportFormat.GlbPrimitive => "An open-source alternative to FBX. Binary version of GLTF. Only contains mesh data. Unity cannot import assets of this type.",
			_ => null,
		};
	}
}