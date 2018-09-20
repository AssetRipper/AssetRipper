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

			m_vectorParams = reader.ReadArray<VectorParameter>();
			m_matrixParams = reader.ReadArray<MatrixParameter>();
			m_textureParams = reader.ReadArray<TextureParameter>();
			m_bufferParams = reader.ReadArray<BufferBinding>();
			m_constantBuffers = reader.ReadArray<ConstantBuffer>();
			m_constantBufferBindings = reader.ReadArray<BufferBinding>();
			m_UAVParams = reader.ReadArray<UAVParameter>();

			if(IsReadSamplers(reader.Version))
			{
				m_samplers = reader.ReadArray<SamplerParameter>();
			}
			if(IsReadShaderRequirements(reader.Version))
			{
				ShaderRequirements = reader.ReadInt32();
			}
		}

		public void Export(TextWriter writer, ShaderSubProgramBlob blob, Platform platform, bool isTier, Func<ShaderGpuProgramType, ShaderTextExporter> exporterInstantiator)
		{
			writer.WriteIntent(4);
			writer.Write("SubProgram \"{0} ", GpuProgramType.ToGPUPlatform(platform));
			if(isTier)
			{
				writer.Write("hw_tier{0} ", ShaderHardwareTier.ToString("00"));
			}
			writer.Write("\" {\n");
			writer.WriteIntent(5);
			
			blob.SubPrograms[(int)BlobIndex].Export(writer, exporterInstantiator);

			writer.Write('\n');
			writer.WriteIntent(4);
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
