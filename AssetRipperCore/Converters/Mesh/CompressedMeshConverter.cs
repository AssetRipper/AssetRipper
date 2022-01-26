using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Math.PackedBitVectors;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.Mesh
{
	public static class CompressedMeshConverter
	{
		public static CompressedMesh Convert(IExportContainer container, CompressedMesh origin)
		{
			CompressedMesh instance = new CompressedMesh();
			instance.Vertices = origin.Vertices.Convert(container);
			instance.UV = origin.UV.Convert(container);
			if (CompressedMesh.HasBindPoses(container.ExportVersion))
			{
				instance.BindPoses = origin.BindPoses.Convert(container);
			}
			instance.Normals = origin.Normals.Convert(container);
			instance.Tangents = origin.Tangents.Convert(container);
			instance.Weights = origin.Weights.Convert(container);
			instance.NormalSigns = origin.NormalSigns.Convert(container);
			instance.TangentSigns = origin.TangentSigns.Convert(container);
			if (CompressedMesh.HasFloatColors(container.ExportVersion))
			{
				instance.FloatColors = GetFloatColors(container, origin);
			}
			instance.BoneIndices = origin.BoneIndices.Convert(container);
			instance.Triangles = origin.Triangles.Convert(container);
			if (CompressedMesh.HasColors(container.ExportVersion))
			{
				instance.Colors = GetColors(container, origin);
			}
			if (CompressedMesh.HasUVInfo(container.ExportVersion))
			{
				instance.UVInfo = GetUVInfo(container, origin);
			}
			return instance;
		}

		private static PackedFloatVector GetFloatColors(IExportContainer container, CompressedMesh origin)
		{
			if (CompressedMesh.HasFloatColors(container.Version))
			{
				return origin.FloatColors.Convert(container);
			}
			else
			{
#warning TODO: convert Colors to FloatColors
				if (origin.Colors.IsSet())
				{
					Logger.Log(LogType.Warning, LogCategory.Export, "Color conversion isn't implemented");
				}
				return new PackedFloatVector();
			}
		}

		private static PackedIntVector GetColors(IExportContainer container, CompressedMesh origin)
		{
			if (CompressedMesh.HasColors(container.Version))
			{
				return origin.Colors.Convert(container);
			}
			return new PackedIntVector();
		}

		private static uint GetUVInfo(IExportContainer container, CompressedMesh origin)
		{
			if (CompressedMesh.HasUVInfo(container.Version))
			{
				return origin.UVInfo;
			}
			return 0;
		}
	}
}
