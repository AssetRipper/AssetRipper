using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.Core.IO.Asset;
using AssetRipper.VersionUtilities;
using ShaderTextRestorer.ShaderBlob.Parameters;
using System;
using System.Collections.Generic;


namespace ShaderTextRestorer.ShaderBlob
{
	public sealed class ShaderSubProgram : IAssetReadable, IAssetWritable
	{
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasLocalKeywords(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasUAVParameters(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasSamplerParameters(UnityVersion version) => version.IsGreaterEqual(2017, 1);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasMultiSampled(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool HasStatsTempRegister(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool IsAllParamArgs(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		private static bool HasStructParameters(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool HasNewTextureParams(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasMergedKeywords(UnityVersion version) => version.IsGreaterEqual(2021, 2);
		private static int GetExpectedProgramVersion(UnityVersion version)
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
			else if (version.IsLess(2021, 2))
			{
				return 201806140;
			}
			else
			{
				return 202012090;
			}
		}

		public void Read(AssetReader reader)
		{
			int version = reader.ReadInt32();
			if (version != GetExpectedProgramVersion(reader.Version))
			{
				throw new Exception($"Shader program version {version} doesn't match");
			}

			ProgramType = reader.ReadInt32();
			StatsALU = reader.ReadInt32();
			StatsTEX = reader.ReadInt32();
			StatsFlow = reader.ReadInt32();
			if (HasStatsTempRegister(reader.Version))
			{
				StatsTempRegister = reader.ReadInt32();
			}

			if (HasMergedKeywords(reader.Version))
			{
				reader.ReadStringArray();
			}
			else
			{
				GlobalKeywords = reader.ReadStringArray();
				if (HasLocalKeywords(reader.Version))
				{
					LocalKeywords = reader.ReadStringArray();
				}
			}

			ProgramData = reader.ReadByteArray();
			reader.AlignStream();

			int sourceMap = reader.ReadInt32();
			int bindCount = reader.ReadInt32();
			ShaderBindChannel[] channels = new ShaderBindChannel[bindCount];
			for (int i = 0; i < bindCount; i++)
			{
				uint source = reader.ReadUInt32();
				VertexComponent target = (VertexComponent)reader.ReadUInt32();
				ShaderBindChannel channel = new ShaderBindChannel(source, target);
				channels[i] = channel;
				sourceMap |= 1 << (int)source;
			}
			BindChannels = new ParserBindChannels(channels, sourceMap);

			List<VectorParameter> vectors = new List<VectorParameter>();
			List<MatrixParameter> matrices = new List<MatrixParameter>();
			List<TextureParameter> textures = new List<TextureParameter>();
			List<VectorParameter> structVectors = new List<VectorParameter>();
			List<MatrixParameter> structMatrices = new List<MatrixParameter>();
			List<BufferBinding> buffers = new List<BufferBinding>();
			List<UAVParameter>? uavs = HasUAVParameters(reader.Version) ? new List<UAVParameter>() : null;
			List<SamplerParameter>? samplers = HasSamplerParameters(reader.Version) ? new List<SamplerParameter>() : null;
			List<BufferBinding> constBindings = new List<BufferBinding>();
			List<StructParameter> structs = new List<StructParameter>();

			int paramGroupCount = reader.ReadInt32();
			ConstantBuffers = new ConstantBuffer[paramGroupCount - 1];
			for (int i = 0; i < paramGroupCount; i++)
			{
				vectors.Clear();
				matrices.Clear();
				structs.Clear();

				string name = reader.ReadString();
				int usedSize = reader.ReadInt32();
				int paramCount = reader.ReadInt32();
				for (int j = 0; j < paramCount; j++)
				{
					string paramName = reader.ReadString();
					ShaderParamType paramType = (ShaderParamType)reader.ReadInt32();
					int rows = reader.ReadInt32();
					int columns = reader.ReadInt32();
					bool isMatrix = reader.ReadInt32() > 0;
					int arraySize = reader.ReadInt32();
					int index = reader.ReadInt32();

					if (isMatrix)
					{
						MatrixParameter matrix = IsAllParamArgs(reader.Version)
							? new MatrixParameter(paramName, paramType, index, arraySize, rows, columns)
							: new MatrixParameter(paramName, paramType, index, rows, columns);
						matrices.Add(matrix);
					}
					else
					{
						VectorParameter vector = IsAllParamArgs(reader.Version)
							? new VectorParameter(paramName, paramType, index, arraySize, columns)
							: new VectorParameter(paramName, paramType, index, columns);
						vectors.Add(vector);
					}
				}

				if (HasStructParameters(reader.Version))
				{
					int structCount = reader.ReadInt32();
					for (int j = 0; j < structCount; j++)
					{
						structVectors.Clear();
						structMatrices.Clear();

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
							int columns = reader.ReadInt32();
							bool isMatrix = reader.ReadInt32() > 0;
							int vectorArraySize = reader.ReadInt32();
							int paramIndex = reader.ReadInt32();

							if (isMatrix)
							{
								MatrixParameter matrix = IsAllParamArgs(reader.Version)
									? new MatrixParameter(paramName, paramType, paramIndex, vectorArraySize, rows, columns)
									: new MatrixParameter(paramName, paramType, paramIndex, rows, columns);
								structMatrices.Add(matrix);
							}
							else
							{
								VectorParameter vector = IsAllParamArgs(reader.Version)
									? new VectorParameter(paramName, paramType, paramIndex, vectorArraySize, columns)
									: new VectorParameter(paramName, paramType, paramIndex, columns);
								structVectors.Add(vector);
							}
						}

						StructParameter @struct = new StructParameter(structName, index, arraySize, structSize, structVectors.ToArray(), structMatrices.ToArray());
						structs.Add(@struct);
					}
				}
				if (i == 0)
				{
					VectorParameters = vectors.ToArray();
					MatrixParameters = matrices.ToArray();
					StructParameters = structs.ToArray();
				}
				else
				{
					ConstantBuffer constBuffer = new ConstantBuffer(name, matrices.ToArray(), vectors.ToArray(), structs.ToArray(), usedSize);
					ConstantBuffers[i - 1] = constBuffer;
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
					if (HasNewTextureParams(reader.Version))
					{
						uint textureExtraValue = reader.ReadUInt32();
						bool isMultiSampled = (textureExtraValue & 1) == 1;
						byte dimension = (byte)(textureExtraValue >> 1);
						int samplerIndex = extraValue;
						texture = new TextureParameter(name, index, dimension, samplerIndex, isMultiSampled);
					}
					else if (HasMultiSampled(reader.Version))
					{
						uint textureExtraValue = reader.ReadUInt32();
						bool isMultiSampled = textureExtraValue == 1;
						byte dimension = unchecked((byte)extraValue);
						int samplerIndex = extraValue >> 8;
						if (samplerIndex == 0xFFFFFF)
						{
							samplerIndex = -1;
						}

						texture = new TextureParameter(name, index, dimension, samplerIndex, isMultiSampled);
					}
					else
					{
						byte dimension = unchecked((byte)extraValue);
						int samplerIndex = extraValue >> 8;
						if (samplerIndex == 0xFFFFFF)
						{
							samplerIndex = -1;
						}

						texture = new TextureParameter(name, index, dimension, samplerIndex);
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
				else if (type == 3 && HasUAVParameters(reader.Version))
				{
					UAVParameter uav = new UAVParameter(name, index, extraValue);
					uavs.Add(uav);
				}
				else if (type == 4 && HasSamplerParameters(reader.Version))
				{
					SamplerParameter sampler = new SamplerParameter((uint)extraValue, index);
					samplers.Add(sampler);
				}
				else
				{
					throw new Exception($"Unupported parameter type {type}");
				}
			}
			TextureParameters = textures.ToArray();
			BufferParameters = buffers.ToArray();
			if (HasUAVParameters(reader.Version))
			{
				UAVParameters = uavs.ToArray();
			}

			if (HasSamplerParameters(reader.Version))
			{
				SamplerParameters = samplers.ToArray();
			}

			ConstantBufferBindings = constBindings.ToArray();
			if (HasStructParameters(reader.Version))
			{
				StructParameters = structs.ToArray();
			}
		}

		public void Write(AssetWriter writer)
		{
#warning TODO:
			throw new NotImplementedException();
		}

		public ShaderGpuProgramType GetProgramType(UnityVersion version)
		{
			if (ShaderGpuProgramTypeExtensions.GpuProgramType55Relevant(version))
			{
				return ((ShaderGpuProgramType55)ProgramType).ToGpuProgramType();
			}
			else
			{
				return ((ShaderGpuProgramType53)ProgramType).ToGpuProgramType();
			}
		}

		public int ProgramType { get; set; }
		public int StatsALU { get; set; }
		public int StatsTEX { get; set; }
		public int StatsFlow { get; set; }
		public int StatsTempRegister { get; set; }
		public string[] GlobalKeywords { get; set; } = Array.Empty<string>();
		public string[] LocalKeywords { get; set; } = Array.Empty<string>();
		public byte[] ProgramData { get; set; } = Array.Empty<byte>();
		public VectorParameter[] VectorParameters { get; set; } = Array.Empty<VectorParameter>();
		public MatrixParameter[] MatrixParameters { get; set; } = Array.Empty<MatrixParameter>();
		public TextureParameter[] TextureParameters { get; set; } = Array.Empty<TextureParameter>();
		public BufferBinding[] BufferParameters { get; set; } = Array.Empty<BufferBinding>();
		public UAVParameter[] UAVParameters { get; set; } = Array.Empty<UAVParameter>();
		public SamplerParameter[] SamplerParameters { get; set; } = Array.Empty<SamplerParameter>();
		public ConstantBuffer[] ConstantBuffers { get; set; } = Array.Empty<ConstantBuffer>();
		public BufferBinding[] ConstantBufferBindings { get; set; } = Array.Empty<BufferBinding>();
		public StructParameter[] StructParameters { get; set; } = Array.Empty<StructParameter>();
		public ParserBindChannels BindChannels { get; set; } = new();
	}
}
