using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedSubProgram : IAssetReadable
	{
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadLocalKeywordIndices(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadSamplers(Version version)
		{
			return version.IsGreaterEqual(2017, 1);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadShaderRequirements(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlignKeywordIndices(Version version)
		{
			return version.IsGreaterEqual(2017, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			// KeywordIndices has been renamed to GlobalKeywordIndices
			if (version.IsGreaterEqual(2019))
			{
				return 3;
			}

			// TODO:
			return 2;
			// return 1;
		}

		public void Read(AssetReader reader)
		{
			BlobIndex = reader.ReadUInt32();
			Channels.Read(reader);
			m_globalKeywordIndices = reader.ReadUInt16Array();
			if (IsAlignKeywordIndices(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadLocalKeywordIndices(reader.Version))
			{
				m_localKeywordIndices = reader.ReadUInt16Array();
				reader.AlignStream(AlignType.Align4);
			}

			ShaderHardwareTier = reader.ReadByte();
			GpuProgramType = (ShaderGpuProgramType)reader.ReadByte();
			reader.AlignStream(AlignType.Align4);

			m_vectorParams = reader.ReadAssetArray<VectorParameter>();
			m_matrixParams = reader.ReadAssetArray<MatrixParameter>();
			m_textureParams = reader.ReadAssetArray<TextureParameter>();
			m_bufferParams = reader.ReadAssetArray<BufferBinding>();
			m_constantBuffers = reader.ReadAssetArray<ConstantBuffer>();
			m_constantBufferBindings = reader.ReadAssetArray<BufferBinding>();
			m_UAVParams = reader.ReadAssetArray<UAVParameter>();

			if (IsReadSamplers(reader.Version))
			{
				m_samplers = reader.ReadAssetArray<SamplerParameter>();
			}
			if (IsReadShaderRequirements(reader.Version))
			{
				ShaderRequirements = reader.ReadInt32();
			}
		}

		public void Export(ShaderWriter writer, ShaderSubProgramBlob blob, ShaderType type, bool isTier)
		{
			writer.WriteIndent(4);
			writer.Write("SubProgram \"{0} ", GpuProgramType.ToGPUPlatform(writer.Platform));
			if (isTier)
			{
				writer.Write("hw_tier{0} ", ShaderHardwareTier.ToString("00"));
			}
			writer.Write("\" {\n");
			writer.WriteIndent(5);

			blob.SubPrograms[(int)BlobIndex].Export(writer, type);

			writer.Write('\n');
			writer.WriteIndent(4);
			writer.Write("}\n");
		}

		public uint BlobIndex { get; private set; }
		/// <summary>
		/// KeywordIndices previously
		/// </summary>
		public IReadOnlyList<ushort> GlobalKeywordIndices => m_globalKeywordIndices;
		public IReadOnlyList<ushort> LocalKeywordIndices => m_localKeywordIndices;
		public byte ShaderHardwareTier { get; private set; }
		public ShaderGpuProgramType GpuProgramType { get; private set; }
		public IReadOnlyList<VectorParameter> VectorParams => m_vectorParams;
		public IReadOnlyList<MatrixParameter> MatrixParams => m_matrixParams;
		public IReadOnlyList<TextureParameter> TextureParams => m_textureParams;
		public IReadOnlyList<BufferBinding> BufferParams => m_bufferParams;
		public IReadOnlyList<ConstantBuffer> ConstantBuffers => m_constantBuffers;
		public IReadOnlyList<BufferBinding> ConstantBufferBindings => m_constantBufferBindings;
		public IReadOnlyList<UAVParameter> UAVParams => m_UAVParams;
		public IReadOnlyList<SamplerParameter> Samplers => m_samplers;
		public int ShaderRequirements { get; private set; }

		public ParserBindChannels Channels;

		private ushort[] m_globalKeywordIndices;
		private ushort[] m_localKeywordIndices;
		private VectorParameter[] m_vectorParams;
		private MatrixParameter[] m_matrixParams;
		private TextureParameter[] m_textureParams;
		private BufferBinding[] m_bufferParams;
		private ConstantBuffer[] m_constantBuffers;
		private BufferBinding[] m_constantBufferBindings;
		private UAVParameter[] m_UAVParams;
		private SamplerParameter[] m_samplers;
	}
}
