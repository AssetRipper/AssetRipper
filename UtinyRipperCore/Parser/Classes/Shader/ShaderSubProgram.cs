using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes.Shaders.Exporters;

namespace UtinyRipper.Classes.Shaders
{
	public struct ShaderSubProgram : IAssetReadable
	{
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
			return version.IsGreaterEqual(2017, 2);
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
			else if (version.IsGreaterEqual(5, 6) && version.IsLess(2017, 3))
			{
				return 201609010;
			}
			else if (version.IsLessEqual(2018, 1))
			{
				return 201708220;
			}
			else if(version.IsLessEqual(2018, 2))
			{
				return 201802150;
			}
			else
			{
				throw new NotSupportedException($"No magic number for version {version}");
			}
		}

		public void Read(AssetStream stream)
		{
			int magic = stream.ReadInt32();
			if (magic != GetMagicNumber(stream.Version))
			{
				throw new Exception($"Magic number {magic} doesn't match");
			}

			ProgramType = (ShaderGpuProgramType)stream.ReadInt32();
			int unknown1 = stream.ReadInt32();
			int unknown2 = stream.ReadInt32();
			int unknown3 = stream.ReadInt32();
			if(IsReadUnknown4(stream.Version))
			{
				int unknown4 = stream.ReadInt32();
			}

			m_keywords = stream.ReadStringArray();
			m_programData = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);

			int sourceMap = stream.ReadInt32();
			int bindCount = stream.ReadInt32();
			ShaderBindChannel[] channels = new ShaderBindChannel[bindCount];
			for (int i = 0; i < bindCount; i++)
			{
				ShaderChannel source = (ShaderChannel)stream.ReadUInt32();
				VertexComponent target = (VertexComponent)stream.ReadUInt32();
				ShaderBindChannel channel = new ShaderBindChannel(source, target);
				channels[i] = channel;
				sourceMap |= 1 << (int)source;
			}
			BindChannels = new ParserBindChannels(channels, sourceMap);

			List<VectorParameter> vectors = new List<VectorParameter>();
			List<MatrixParameter> matrices = new List<MatrixParameter>();
			List<TextureParameter> textures = new List<TextureParameter>();
			List<BufferBinding> buffers = new List<BufferBinding>();
			List<UAVParameter> uavs = IsReadUAVParameters(stream.Version) ? new List<UAVParameter>() : null;
			List<SamplerParameter> samplers = IsReadSamplerParameters(stream.Version) ? new List<SamplerParameter>() : null;
			List<BufferBinding> constBindings = new List<BufferBinding>();
			List<StructParameter> structs = IsReadStructParameters(stream.Version) ? new List<StructParameter>() : null;

			int paramGroupCount = stream.ReadInt32();
			m_constantBuffers = new ConstantBuffer[paramGroupCount - 1];
			for (int i = 0; i < paramGroupCount; i++)
			{
				vectors.Clear();
				matrices.Clear();

				string name = stream.ReadStringAligned();
				int usedSize = stream.ReadInt32();
				int paramCount = stream.ReadInt32();
				for (int j = 0; j < paramCount; j++)
				{
					string paramName = stream.ReadStringAligned();
					ShaderParamType paramType = (ShaderParamType)stream.ReadInt32();
					int rows = stream.ReadInt32();
					int dimension = stream.ReadInt32();
					bool isMatrix = stream.ReadInt32() > 0;
					int arraySize = stream.ReadInt32();
					int index = stream.ReadInt32();

					if (isMatrix)
					{
						MatrixParameter matrix = IsAllParamArgs(stream.Version) ?
							new MatrixParameter(paramName, paramType, index, arraySize, rows) :
							new MatrixParameter(paramName, paramType, index, rows);
						matrices.Add(matrix);
					}
					else
					{
						VectorParameter vector = IsAllParamArgs(stream.Version) ?
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

				if (IsReadStructParameters(stream.Version))
				{
					int structCount = stream.ReadInt32();
					for(int j = 0; j < structCount; j++)
					{
						vectors.Clear();
						matrices.Clear();

						string structName = stream.ReadStringAligned();
						int index = stream.ReadInt32();
						int arraySize = stream.ReadInt32();
						int structSize = stream.ReadInt32();

						int strucParamCount = stream.ReadInt32();
						for(int k = 0; k < strucParamCount; k++)
						{
							string paramName = stream.ReadStringAligned();
							paramName = $"{structName}.{paramName}";
							ShaderParamType paramType = (ShaderParamType)stream.ReadInt32();
							int rows = stream.ReadInt32();
							int dimension = stream.ReadInt32();
							bool isMatrix = stream.ReadInt32() > 0;
							int vectorArraySize = stream.ReadInt32();
							int paramIndex = stream.ReadInt32();

							if (isMatrix)
							{
								MatrixParameter matrix = IsAllParamArgs(stream.Version) ?
									new MatrixParameter(paramName, paramType, paramIndex, vectorArraySize, rows) :
									new MatrixParameter(paramName, paramType, paramIndex, rows);
								matrices.Add(matrix);
							}
							else
							{
								VectorParameter vector = IsAllParamArgs(stream.Version) ?
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

			int paramGroup2Count = stream.ReadInt32();
			for (int i = 0; i < paramGroup2Count; i++)
			{
				string name = stream.ReadStringAligned();
				int type = stream.ReadInt32();
				int index = stream.ReadInt32();
				int extraValue = stream.ReadInt32();

				if (type == 0)
				{
					TextureParameter texture;
					if (IsReadMultiSampled(stream.Version))
					{
						bool isMultiSampled = stream.ReadUInt32() > 0;
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
				else if(type == 3 && IsReadUAVParameters(stream.Version))
				{
					UAVParameter uav = new UAVParameter(name, index, extraValue);
					uavs.Add(uav);
				}
				else if(type == 4 && IsReadSamplerParameters(stream.Version))
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
			if(IsReadUAVParameters(stream.Version))
			{
				m_UAVParameters = uavs.ToArray();
			}
			if(IsReadSamplerParameters(stream.Version))
			{
				m_samplerParameters = samplers.ToArray();
			}
			m_constantBufferBindings = constBindings.ToArray();
			if(IsReadStructParameters(stream.Version))
			{
				m_structParameters = structs.ToArray();
			}
		}
		
		public void Export(TextWriter writer, Func<ShaderGpuProgramType, ShaderTextExporter> exporterInstantiator)
		{
			if(Keywords.Count > 0)
			{
				writer.Write("Keywords { ");
				foreach(string keyword in Keywords)
				{
					writer.Write("\"{0}\" ", keyword);
				}
				writer.Write("}\n");
				writer.WriteIntent(5);
			}

			if(m_programData.Length == 0)
			{
				return;
			}

			writer.Write("\"!!{0}\n", ProgramType.ToString());
			writer.WriteIntent(5);

			using (MemoryStream stream = new MemoryStream(m_programData))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					ShaderTextExporter exporter = exporterInstantiator.Invoke(ProgramType);
					exporter.Export(reader, writer);
				}
			}
			writer.Write('"');
		}
		
		public ShaderGpuProgramType ProgramType { get; private set; }
		public IReadOnlyList<string> Keywords => m_keywords;
		public IReadOnlyList<byte> ProgramData => m_programData;
		public IReadOnlyList<VectorParameter> VectorParameters => m_vectorParameters;
		public IReadOnlyList<MatrixParameter> MatrixParameters => m_matrixParameters;
		public IReadOnlyList<TextureParameter> TextureParameters => m_textureParameters;
		public IReadOnlyList<BufferBinding> BufferParameters => m_bufferParameters;
		public IReadOnlyList<UAVParameter> UAVParameters => m_UAVParameters;
		public IReadOnlyList<ConstantBuffer> ConstantBuffers => m_constantBuffers;
		public IReadOnlyList<BufferBinding> ConstantBufferBindings => m_constantBufferBindings;
		public IReadOnlyList<StructParameter> StructParameters => m_structParameters;

		public ParserBindChannels BindChannels;

		private string[] m_keywords;
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
