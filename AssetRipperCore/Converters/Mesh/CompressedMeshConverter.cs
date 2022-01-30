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
			instance.Vertices.CopyValuesFrom(origin.Vertices);
			instance.UV.CopyValuesFrom(origin.UV);
			if (CompressedMesh.HasBindPoses(container.ExportVersion))
			{
				instance.BindPoses.CopyValuesFrom(origin.BindPoses);
			}
			instance.Normals.CopyValuesFrom(origin.Normals);
			instance.Tangents.CopyValuesFrom(origin.Tangents);
			instance.Weights.CopyValuesFrom(origin.Weights);
			instance.NormalSigns.CopyValuesFrom(origin.NormalSigns);
			instance.TangentSigns.CopyValuesFrom(origin.TangentSigns);
			if (CompressedMesh.HasFloatColors(container.ExportVersion))
			{
				instance.FloatColors.CopyValuesFrom(GetFloatColors(container, origin));
			}
			instance.BoneIndices.CopyValuesFrom(origin.BoneIndices);
			instance.Triangles.CopyValuesFrom(origin.Triangles);
			if (CompressedMesh.HasColors(container.ExportVersion))
			{
				instance.Colors.CopyValuesFrom(GetColors(container, origin));
			}
			if (CompressedMesh.HasUVInfo(container.ExportVersion))
			{
				instance.UVInfo = GetUVInfo(container, origin);
			}
			return instance;
		}

		private static IPackedFloatVector GetFloatColors(IExportContainer container, CompressedMesh origin)
		{
			if (CompressedMesh.HasFloatColors(container.Version))
			{
				return origin.FloatColors;
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

		private static IPackedIntVector GetColors(IExportContainer container, CompressedMesh origin)
		{
			if (CompressedMesh.HasColors(container.Version))
			{
				return origin.Colors;
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
