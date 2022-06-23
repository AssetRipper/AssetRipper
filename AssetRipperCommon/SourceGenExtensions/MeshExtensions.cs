using AssetRipper.Core.Classes.Mesh;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShape;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MeshExtensions
	{
		public static bool IsCombinedMesh(this IMesh mesh) => mesh?.NameString == "Combined Mesh (root scene)";

		public static void ConvertToEditorFormat(this IMesh mesh)
		{
			mesh.SetMeshOptimizationFlags(MeshOptimizationFlags.Everything);
		}

		public static bool CheckAssetIntegrity(this IMesh mesh)
		{
			if (mesh.Has_StreamData_C43() && mesh.Has_VertexData_C43() && mesh.SerializedFile is not null)
			{
				if (mesh.VertexData_C43.IsSet())
				{
					return mesh.StreamData_C43.CheckIntegrity(mesh.SerializedFile);
				}
			}
			return true;
		}

		public static byte[] GetChannelsData(this IMesh mesh)
		{
			if (mesh.Has_StreamData_C43() && mesh.StreamData_C43.IsSet())
			{
				return mesh.StreamData_C43.GetContent(mesh.SerializedFile);
			}
			else
			{
				return mesh.VertexData_C43?.Data ?? Array.Empty<byte>();
			}
		}

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
				return mesh.IndexFormat_C43 == (int)IndexFormat.UInt16;
			}
			return true;
		}

		public static MeshOptimizationFlags GetMeshOptimizationFlags(this IMesh mesh)
		{
			if (mesh.Has_MeshOptimizationFlags_C43())
			{
				return (MeshOptimizationFlags)mesh.MeshOptimizationFlags_C43;
			}
			else if (mesh.Has_MeshOptimized_C43())
			{
				return mesh.MeshOptimized_C43 ? MeshOptimizationFlags.Everything : MeshOptimizationFlags.PolygonOrder;
			}
			else
			{
				return default;
			}
		}

		public static void SetMeshOptimizationFlags(this IMesh mesh, MeshOptimizationFlags value)
		{
			if (mesh.Has_MeshOptimizationFlags_C43())
			{
				mesh.MeshOptimizationFlags_C43 = (int)value;
			}
			else if (mesh.Has_MeshOptimized_C43())
			{
				mesh.MeshOptimized_C43 = value == MeshOptimizationFlags.Everything;
			}
		}
	}
}
