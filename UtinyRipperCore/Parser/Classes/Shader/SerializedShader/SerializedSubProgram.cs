using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes.Shaders.Exporters;

namespace UtinyRipper.Classes.Shaders
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

		public void Read(AssetStream stream)
		{
			BlobIndex = stream.ReadUInt32();
			Channels.Read(stream);
			m_keywordIndices = stream.ReadUInt16Array();
			if(IsAlignKeywordIndices(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			ShaderHardwareTier = stream.ReadByte();
			GpuProgramType = (ShaderGpuProgramType)stream.ReadByte();
			stream.AlignStream(AlignType.Align4);

			m_vectorParams = stream.ReadArray<VectorParameter>();
			m_matrixParams = stream.ReadArray<MatrixParameter>();
			m_textureParams = stream.ReadArray<TextureParameter>();
			m_bufferParams = stream.ReadArray<BufferBinding>();
			m_constantBuffers = stream.ReadArray<ConstantBuffer>();
			m_constantBufferBindings = stream.ReadArray<BufferBinding>();
			m_UAVParams = stream.ReadArray<UAVParameter>();

			if(IsReadSamplers(stream.Version))
			{
				m_samplers = stream.ReadArray<SamplerParameter>();
			}
			if(IsReadShaderRequirements(stream.Version))
			{
				ShaderRequirements = stream.ReadInt32();
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
