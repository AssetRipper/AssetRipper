using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.PackedBitVectors;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface ICompressedMesh : IAsset
	{
		IPackedFloatVector Vertices { get; }
		IPackedFloatVector UV { get; }
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		IPackedFloatVector BindPoses { get; }
		IPackedFloatVector Normals { get; }
		IPackedFloatVector Tangents { get; }
		IPackedIntVector Weights { get; }
		IPackedIntVector NormalSigns { get; }
		IPackedIntVector TangentSigns { get; }
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		IPackedFloatVector FloatColors { get; }
		IPackedIntVector BoneIndices { get; }
		IPackedIntVector Triangles { get; }
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		IPackedIntVector Colors { get; }
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		uint UVInfo { get; set; }
	}

	public static class CompressedMeshExtensions
	{
		public static void CopyValues(this ICompressedMesh destination, ICompressedMesh source)
		{
			destination.Vertices.CopyValuesFrom(source.Vertices);
			destination.UV.CopyValuesFrom(source.UV);
			destination.BindPoses.CopyValuesFrom(source.BindPoses);
			destination.Normals.CopyValuesFrom(source.Normals);
			destination.Tangents.CopyValuesFrom(source.Tangents);
			destination.Weights.CopyValuesFrom(source.Weights);
			destination.NormalSigns.CopyValuesFrom(source.NormalSigns);
			destination.TangentSigns.CopyValuesFrom(source.TangentSigns);
			destination.FloatColors.CopyValuesFrom(source.FloatColors);
			destination.BoneIndices.CopyValuesFrom(source.BoneIndices);
			destination.Triangles.CopyValuesFrom(source.Triangles);
			destination.Colors.CopyValuesFrom(source.Colors);
			destination.UVInfo = source.UVInfo;
		}
	}
}
