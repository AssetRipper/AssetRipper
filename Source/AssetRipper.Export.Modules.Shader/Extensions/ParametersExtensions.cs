using AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using Subclasses = AssetRipper.SourceGenerated.Subclasses;

namespace AssetRipper.Export.Modules.Shaders.Extensions
{
	public static class ParametersExtensions
	{
		public static BufferBinding ToBufferBinding(this Subclasses.BufferBinding.IBufferBinding _this, IReadOnlyDictionary<int, string> nameIndices)
		{
			return new BufferBinding(nameIndices[_this.NameIndex], _this.Index)
			{
				ArraySize = _this.ArraySize
			};
		}

		public static ConstantBuffer ToConstantBuffer(this Subclasses.ConstantBuffer.IConstantBuffer _this, IReadOnlyDictionary<int, string> nameIndices)
		{
			return new ConstantBuffer(nameIndices[_this.NameIndex],
										_this.MatrixParams.Select(item => item.ToMatrixParameter(nameIndices)).ToArray(),
										_this.VectorParams.Select(item => item.ToVectorParameter(nameIndices)).ToArray(),
										_this.StructParams?.Select(item => item.ToStructParameter(nameIndices))?.ToArray() ?? Array.Empty<StructParameter>(),
										_this.Size)
			{
				IsPartialCB = _this.IsPartialCB
			};
		}

		public static SamplerParameter ToSamplerParameter(this Subclasses.SamplerParameter.ISamplerParameter _this)
		{
			return new SamplerParameter(_this.Sampler, _this.BindPoint);
		}

		public static TextureParameter ToTextureParameter(this Subclasses.TextureParameter.ITextureParameter _this, IReadOnlyDictionary<int, string> nameIndices)
		{
			return new TextureParameter(nameIndices[_this.NameIndex], _this.Index, (byte)_this.Dim, _this.SamplerIndex, _this.MultiSampled);
		}

		public static MatrixParameter ToMatrixParameter(this Subclasses.MatrixParameter.IMatrixParameter _this, IReadOnlyDictionary<int, string> nameIndices)
		{
			// ColumnCount does not exist in IMatrixParameter
			return new MatrixParameter(nameIndices[_this.NameIndex], (ShaderParamType)_this.Type, _this.Index, _this.ArraySize, _this.RowCount, 1);
		}

		public static UAVParameter ToUAVParameter(this Subclasses.UAVParameter.IUAVParameter _this, IReadOnlyDictionary<int, string> nameIndices)
		{
			return new UAVParameter(nameIndices[_this.NameIndex], _this.Index, _this.OriginalIndex);
		}

		public static VectorParameter ToVectorParameter(this Subclasses.VectorParameter.IVectorParameter _this, IReadOnlyDictionary<int, string> nameIndices)
		{
			return new VectorParameter(nameIndices[_this.NameIndex], (ShaderParamType)_this.Type, _this.Index, _this.ArraySize, _this.Dim);
		}


		public static StructParameter ToStructParameter(this Subclasses.StructParameter.IStructParameter _this, IReadOnlyDictionary<int, string> nameIndices)
		{
			return new StructParameter(nameIndices[_this.NameIndex], _this.Index, _this.ArraySize, _this.StructSize,
										_this.VectorMembers.Select(item => item.ToVectorParameter(nameIndices)).ToArray(),
										_this.MatrixMembers.Select(item => item.ToMatrixParameter(nameIndices)).ToArray());
		}

	}
}
