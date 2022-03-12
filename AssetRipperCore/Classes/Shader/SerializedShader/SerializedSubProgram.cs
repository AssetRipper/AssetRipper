using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.Core.Classes.Shader.Parameters;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedSubProgram : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// KeywordIndices has been renamed to GlobalKeywordIndices
			if (version.IsGreaterEqual(2019))
			{
				return 3;
			}

#warning TODO:
			return 2;
			// return 1;
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlignKeywordIndices(UnityVersion version) => version.IsGreaterEqual(2017, 1);

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasLocalKeywordIndices(UnityVersion version) => version.IsGreaterEqual(2019);

		/// <summary>
		/// If on 2021, 2021.1.1 and greater. Otherwise, 2020.3.2 and greater.
		/// Not present in 2021.1.0 - 2021.1.3
		/// </summary>
		public static bool HasUnifiedParameters(UnityVersion version) => version.Major == 2021 ? version.IsGreaterEqual(2021, 1, 1) : version.IsGreaterEqual(2020, 3, 2);

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasSamplers(UnityVersion version) => version.IsGreaterEqual(2017, 1);

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasShaderRequirements(UnityVersion version) => version.IsGreaterEqual(2017, 2);

		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasMergedKeywordIndices(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		/// <summary>
		/// 2021 and greater
		/// </summary>
		private static bool IsShaderRequirementsInt64(UnityVersion version) => version.IsGreaterEqual(2021);

		public void Read(AssetReader reader)
		{
			BlobIndex = reader.ReadUInt32();
			Channels.Read(reader);
			if (HasMergedKeywordIndices(reader.Version))
			{
				GlobalKeywordIndices = reader.ReadUInt16Array();
				reader.AlignStream();
			}
			else
			{
				GlobalKeywordIndices = reader.ReadUInt16Array();
				if (IsAlignKeywordIndices(reader.Version))
				{
					reader.AlignStream();
				}

				if (HasLocalKeywordIndices(reader.Version))
				{
					LocalKeywordIndices = reader.ReadUInt16Array();
					reader.AlignStream();
				}
			}

			ShaderHardwareTier = reader.ReadByte();
			GpuProgramType = reader.ReadByte();
			reader.AlignStream();

			if (HasUnifiedParameters(reader.Version))
			{
				Parameters = reader.ReadAsset<SerializedProgramParameters>();
				VectorParams = Parameters.VectorParams;
				MatrixParams = Parameters.MatrixParams;
				TextureParams = Parameters.TextureParams;
				BufferParams = Parameters.BufferParams;
				ConstantBuffers = Parameters.ConstantBuffers;
				ConstantBufferBindings = Parameters.ConstantBufferBindings;
				UAVParams = Parameters.UAVParams;
				Samplers = Parameters.Samplers;
			}
			else
			{
				VectorParams = reader.ReadAssetArray<VectorParameter>();
				MatrixParams = reader.ReadAssetArray<MatrixParameter>();
				TextureParams = reader.ReadAssetArray<TextureParameter>();
				BufferParams = reader.ReadAssetArray<BufferBinding>();
				ConstantBuffers = reader.ReadAssetArray<ConstantBuffer>();
				ConstantBufferBindings = reader.ReadAssetArray<BufferBinding>();
				UAVParams = reader.ReadAssetArray<UAVParameter>();

				if (HasSamplers(reader.Version))
				{
					Samplers = reader.ReadAssetArray<SamplerParameter>();
				}
			}

			if (HasShaderRequirements(reader.Version))
			{
				if (IsShaderRequirementsInt64(reader.Version))
					ShaderRequirements = reader.ReadInt64();
				else
					ShaderRequirements = reader.ReadInt32();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add("m_BlobIndex", BlobIndex);
			node.Add("m_Channels", Channels.ExportYAML(container));
			if (HasMergedKeywordIndices(container.ExportVersion))
			{
				node.Add("m_KeywordIndices", GlobalKeywordIndices.ExportYAML(false));
			}
			else
			{
				node.Add("m_GlobalKeywordIndices", GlobalKeywordIndices.ExportYAML(false));
				if (HasLocalKeywordIndices(container.ExportVersion))
				{
					node.Add("m_LocalKeywordIndices", LocalKeywordIndices.ExportYAML(false));
				}
			}

			node.Add("m_ShaderHardwareTier", ShaderHardwareTier);
			node.Add("m_GpuProgramType", GpuProgramType);
			if (HasUnifiedParameters(container.ExportVersion))
			{
				node.Add("m_Parameters", Parameters.ExportYAML(container));
			}
			else
			{
				node.Add("m_VectorParams", VectorParams.ExportYAML(container));
				node.Add("m_MatrixParams", MatrixParams.ExportYAML(container));
				node.Add("m_TextureParams", TextureParams.ExportYAML(container));
				node.Add("m_BufferParams", BufferParams.ExportYAML(container));
				node.Add("m_ConstantBuffers", ConstantBuffers.ExportYAML(container));
				node.Add("m_ConstantBufferBindings", ConstantBufferBindings.ExportYAML(container));
				node.Add("m_UAVParams", UAVParams.ExportYAML(container));
				if (HasSamplers(container.ExportVersion))
				{
					node.Add("m_Samplers", Samplers.ExportYAML(container));
				}
			}

			if (HasShaderRequirements(container.ExportVersion))
			{
				if (IsShaderRequirementsInt64(container.ExportVersion))
					node.Add("m_ShaderRequirements", ShaderRequirements);
				else
					node.Add("m_ShaderRequirements", (int)ShaderRequirements);
			}

			return node;
		}

		public ShaderGpuProgramType GetProgramType(UnityVersion version)
		{
			if (ShaderGpuProgramTypeExtensions.GpuProgramType55Relevant(version))
			{
				return ((ShaderGpuProgramType55)GpuProgramType).ToGpuProgramType();
			}
			else
			{
				return ((ShaderGpuProgramType53)GpuProgramType).ToGpuProgramType();
			}
		}

		public uint BlobIndex { get; set; }
		/// <summary>
		/// KeywordIndices previously
		/// </summary>
		public ushort[] GlobalKeywordIndices { get; set; }
		public ushort[] LocalKeywordIndices { get; set; }
		public byte ShaderHardwareTier { get; set; }
		public byte GpuProgramType { get; set; }
		public SerializedProgramParameters Parameters { get; set; }
		public VectorParameter[] VectorParams { get; set; }
		public MatrixParameter[] MatrixParams { get; set; }
		public TextureParameter[] TextureParams { get; set; }
		public BufferBinding[] BufferParams { get; set; }
		public ConstantBuffer[] ConstantBuffers { get; set; }
		public BufferBinding[] ConstantBufferBindings { get; set; }
		public UAVParameter[] UAVParams { get; set; }
		public SamplerParameter[] Samplers { get; set; }
		public long ShaderRequirements { get; set; }

		public ParserBindChannels Channels = new();
	}
}
