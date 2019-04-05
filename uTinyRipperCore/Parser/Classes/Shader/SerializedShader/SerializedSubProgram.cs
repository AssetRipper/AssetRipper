using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Shaders.Exporters;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedSubProgram : IAssetReadable
	{
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

		public void Read(AssetReader reader)
		{
			BlobIndex = reader.ReadUInt32();
			Channels.Read(reader);
			m_keywordIndices = reader.ReadUInt16Array();
			if(IsAlignKeywordIndices(reader.Version))
			{
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

			if(IsReadSamplers(reader.Version))
			{
				m_samplers = reader.ReadAssetArray<SamplerParameter>();
			}
			if(IsReadShaderRequirements(reader.Version))
			{
				ShaderRequirements = reader.ReadInt32();
			}
		}

		public void Export(ShaderWriter writer, ShaderSubProgramBlob blob, ShaderType type, bool isTier)
		{
			writer.WriteIndent(4);
			writer.Write("SubProgram \"{0} ", GpuProgramType.ToGPUPlatform(writer.Platform));
			if(isTier)
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
		public IReadOnlyList<ushort> KeywordIndices => m_keywordIndices;
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

		private ushort[] m_keywordIndices;
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
