using System;
using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderSubProgram : IAssetReadable
	{
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadLocalKeywords(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadUAVParameters(Version version)
		{
			return Shader.IsSerialized(version);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadSamplerParameters(Version version)
		{
			return version.IsGreaterEqual(2017, 1);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadMultiSampled(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool IsReadUnknown4(Version version)
		{
			return Shader.IsSerialized(version);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool IsAllParamArgs(Version version)
		{
			return Shader.IsSerialized(version);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		private static bool IsReadStructParameters(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		private static int GetMagicNumber(Version version)
		{
			if (version.IsEqual(5, 3))
			{
				return 201509030;
			}
			else if (version.IsEqual(5, 4))
			{
				return 201510240;
			}
			else if (version.IsEqual(5, 5))
			{
				return 201608170;
			}
			else if (version.IsLess(2017, 3))
			{
				return 201609010;
			}
			else if (version.IsLess(2018, 2))
			{
				return 201708220;
			}
			else if (version.IsLess(2019))
			{
				return 201802150;
			}
			else
			{
				return 201806140;
			}
		}

		public void Read(AssetReader reader)
		{
			int magic = reader.ReadInt32();
			if (magic != GetMagicNumber(reader.Version))
			{
				throw new Exception($"Magic number {magic} doesn't match");
			}

			ProgramType = (ShaderGpuProgramType)reader.ReadInt32();
			int unknown1 = reader.ReadInt32();
			int unknown2 = reader.ReadInt32();
			int unknown3 = reader.ReadInt32();
			if (IsReadUnknown4(reader.Version))
			{
				int unknown4 = reader.ReadInt32();
			}

			m_globalKeywords = reader.ReadStringArray();
			if (IsReadLocalKeywords(reader.Version))
			{
				m_localKeywords = reader.ReadStringArray();
			}
			m_programData = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);

			int sourceMap = reader.ReadInt32();
			int bindCount = reader.ReadInt32();
			ShaderBindChannel[] channels = new ShaderBindChannel[bindCount];
			for (int i = 0; i < bindCount; i++)
			{
				ShaderChannel source = (ShaderChannel)reader.ReadUInt32();
				VertexComponent target = (VertexComponent)reader.ReadUInt32();
				ShaderBindChannel channel = new ShaderBindChannel(source, target);
				channels[i] = channel;
				sourceMap |= 1 << (int)source;
			}
			BindChannels = new ParserBindChannels(channels, sourceMap);

			List<VectorParameter> vectors = new List<VectorParameter>();
			List<MatrixParameter> matrices = new List<MatrixParameter>();
			List<TextureParameter> textures = new List<TextureParameter>();
			List<BufferBinding> buffers = new List<BufferBinding>();
			List<UAVParameter> uavs = IsReadUAVParameters(reader.Version) ? new List<UAVParameter>() : null;
			List<SamplerParameter> samplers = IsReadSamplerParameters(reader.Version) ? new List<SamplerParameter>() : null;
			List<BufferBinding> constBindings = new List<BufferBinding>();
			List<StructParameter> structs = IsReadStructParameters(reader.Version) ? new List<StructParameter>() : null;

			int paramGroupCount = reader.ReadInt32();
			m_constantBuffers = new ConstantBuffer[paramGroupCount - 1];
			for (int i = 0; i < paramGroupCount; i++)
			{
				vectors.Clear();
				matrices.Clear();

				string name = reader.ReadString();
				int usedSize = reader.ReadInt32();
				int paramCount = reader.ReadInt32();
				for (int j = 0; j < paramCount; j++)
				{
					string paramName = reader.ReadString();
					ShaderParamType paramType = (ShaderParamType)reader.ReadInt32();
					int rows = reader.ReadInt32();
					int dimension = reader.ReadInt32();
					bool isMatrix = reader.ReadInt32() > 0;
					int arraySize = reader.ReadInt32();
					int index = reader.ReadInt32();

					if (isMatrix)
					{
						MatrixParameter matrix = IsAllParamArgs(reader.Version) ?
							new MatrixParameter(paramName, paramType, index, arraySize, rows) :
							new MatrixParameter(paramName, paramType, index, rows);
						matrices.Add(matrix);
					}
					else
					{
						VectorParameter vector = IsAllParamArgs(reader.Version) ?
							new VectorParameter(paramName, paramType, index, arraySize, dimension) :
							new VectorParameter(paramName, paramType, index, dimension);
						vectors.Add(vector);
					}
				}

				if (i == 0)
				{
					m_vectorParameters = vectors.ToArray();
					m_matrixParameters = matrices.ToArray();
				}
				else
				{
					ConstantBuffer constBuffer = new ConstantBuffer(name, matrices.ToArray(), vectors.ToArray(), usedSize);
					m_constantBuffers[i - 1] = constBuffer;
				}

				if (IsReadStructParameters(reader.Version))
				{
					int structCount = reader.ReadInt32();
					for (int j = 0; j < structCount; j++)
					{
						vectors.Clear();
						matrices.Clear();

						string structName = reader.ReadString();
						int index = reader.ReadInt32();
						int arraySize = reader.ReadInt32();
						int structSize = reader.ReadInt32();

						int strucParamCount = reader.ReadInt32();
						for (int k = 0; k < strucParamCount; k++)
						{
							string paramName = reader.ReadString();
							paramName = $"{structName}.{paramName}";
							ShaderParamType paramType = (ShaderParamType)reader.ReadInt32();
							int rows = reader.ReadInt32();
							int dimension = reader.ReadInt32();
							bool isMatrix = reader.ReadInt32() > 0;
							int vectorArraySize = reader.ReadInt32();
							int paramIndex = reader.ReadInt32();

							if (isMatrix)
							{
								MatrixParameter matrix = IsAllParamArgs(reader.Version) ?
									new MatrixParameter(paramName, paramType, paramIndex, vectorArraySize, rows) :
									new MatrixParameter(paramName, paramType, paramIndex, rows);
								matrices.Add(matrix);
							}
							else
							{
								VectorParameter vector = IsAllParamArgs(reader.Version) ?
									new VectorParameter(paramName, paramType, paramIndex, vectorArraySize, dimension) :
									new VectorParameter(paramName, paramType, paramIndex, dimension);
								vectors.Add(vector);
							}
						}

						StructParameter @struct = new StructParameter(structName, index, arraySize, structSize, vectors.ToArray(), matrices.ToArray());
						structs.Add(@struct);
					}
				}
			}

			int paramGroup2Count = reader.ReadInt32();
			for (int i = 0; i < paramGroup2Count; i++)
			{
				string name = reader.ReadString();
				int type = reader.ReadInt32();
				int index = reader.ReadInt32();
				int extraValue = reader.ReadInt32();

				if (type == 0)
				{
					TextureParameter texture;
					if (IsReadMultiSampled(reader.Version))
					{
						bool isMultiSampled = reader.ReadUInt32() > 0;
						texture = new TextureParameter(name, index, isMultiSampled, extraValue);
					}
					else
					{
						texture = new TextureParameter(name, index, extraValue);
					}
					textures.Add(texture);
				}
				else if (type == 1)
				{
					BufferBinding binding = new BufferBinding(name, index);
					constBindings.Add(binding);
				}
				else if (type == 2)
				{
					BufferBinding buffer = new BufferBinding(name, index);
					buffers.Add(buffer);
				}
				else if (type == 3 && IsReadUAVParameters(reader.Version))
				{
					UAVParameter uav = new UAVParameter(name, index, extraValue);
					uavs.Add(uav);
				}
				else if (type == 4 && IsReadSamplerParameters(reader.Version))
				{
					SamplerParameter sampler = new SamplerParameter((uint)extraValue, index);
					samplers.Add(sampler);
				}
				else
				{
					throw new Exception($"Unupported parameter type {type}");
				}
			}
			m_textureParameters = textures.ToArray();
			m_bufferParameters = buffers.ToArray();
			if (IsReadUAVParameters(reader.Version))
			{
				m_UAVParameters = uavs.ToArray();
			}
			if (IsReadSamplerParameters(reader.Version))
			{
				m_samplerParameters = samplers.ToArray();
			}
			m_constantBufferBindings = constBindings.ToArray();
			if (IsReadStructParameters(reader.Version))
			{
				m_structParameters = structs.ToArray();
			}
		}

		public void Export(ShaderWriter writer, ShaderType type)
		{
			if (GlobalKeywords.Count > 0)
			{
				writer.Write("Keywords { ");
				foreach (string keyword in GlobalKeywords)
				{
					writer.Write("\"{0}\" ", keyword);
				}
				if (IsReadLocalKeywords(writer.Version))
				{
					foreach (string keyword in LocalKeywords)
					{
						writer.Write("\"{0}\" ", keyword);
					}
				}
				writer.Write("}\n");
				writer.WriteIndent(5);
			}

			writer.Write("\"!!{0}", ProgramType.ToShaderName(writer.Platform, type));
			if (m_programData.Length > 0)
			{
				writer.Write("\n");
				writer.WriteIndent(5);

				writer.WriteShaderData(ProgramType.ToGPUPlatform(writer.Platform), m_programData);
			}
			writer.Write('"');
		}

		public ShaderGpuProgramType ProgramType { get; private set; }
		/// <summary>
		/// Keywords previously
		/// </summary>
		public IReadOnlyList<string> GlobalKeywords => m_globalKeywords;
		public IReadOnlyList<string> LocalKeywords => m_localKeywords;
		public IReadOnlyList<byte> ProgramData => m_programData;
		public IReadOnlyList<VectorParameter> VectorParameters => m_vectorParameters;
		public IReadOnlyList<MatrixParameter> MatrixParameters => m_matrixParameters;
		public IReadOnlyList<TextureParameter> TextureParameters => m_textureParameters;
		public IReadOnlyList<BufferBinding> BufferParameters => m_bufferParameters;
		public IReadOnlyList<UAVParameter> UAVParameters => m_UAVParameters;
		public IReadOnlyList<SamplerParameter> SamplerParameters => m_samplerParameters;
		public IReadOnlyList<ConstantBuffer> ConstantBuffers => m_constantBuffers;
		public IReadOnlyList<BufferBinding> ConstantBufferBindings => m_constantBufferBindings;
		public IReadOnlyList<StructParameter> StructParameters => m_structParameters;

		public ParserBindChannels BindChannels;

		private string[] m_globalKeywords;
		private string[] m_localKeywords;
		private byte[] m_programData;
		private VectorParameter[] m_vectorParameters;
		private MatrixParameter[] m_matrixParameters;
		private TextureParameter[] m_textureParameters;
		private BufferBinding[] m_bufferParameters;
		private UAVParameter[] m_UAVParameters;
		private SamplerParameter[] m_samplerParameters;
		private ConstantBuffer[] m_constantBuffers;
		private BufferBinding[] m_constantBufferBindings;
		private StructParameter[] m_structParameters;
	}
}
