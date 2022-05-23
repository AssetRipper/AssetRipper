using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShape;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MeshExtensions
	{
		public static bool IsCombinedMesh(this IMesh mesh) => mesh?.NameString == "Combined Mesh (root scene)";

		public static string? FindBlendShapeNameByCRC(this IMesh mesh, uint crc)
		{
			if (mesh.Has_Shapes_C43())
			{
				return mesh.Shapes_C43.FindShapeNameByCRC(crc);
			}
			else if (mesh.Has_ShapesList_C43())
			{
				foreach (MeshBlendShape_4_1_0_f4 blendShape in mesh.ShapesList_C43)
				{
					if (blendShape.IsCRCMatch(crc))
					{
						return blendShape.Name.String;
					}
				}
			}
			return null;
		}

		public static bool Is16BitIndices(this IMesh mesh)
		{
			if (mesh.Has_Use16BitIndices_C43())
			{
				return mesh.Use16BitIndices_C43 != 0;
			}
			else if (mesh.Has_IndexFormat_C43())
			{
				return mesh.IndexFormat_C43 == (int)Classes.Mesh.IndexFormat.UInt16;
			}
			return true;
		}
	}
}
