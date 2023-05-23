using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Assembly.Serializable
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public record struct SerializableField(ulong PValue, object CValue)
	{
		public void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, in SerializableType.Field etalon)
		{
			switch (etalon.Type.Type)
			{
				case PrimitiveType.Bool:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<bool>(version);
					}
					else
					{
						PValue = reader.ReadBoolean() ? 1U : 0U;
					}
					reader.Align();
					break;

				case PrimitiveType.Char:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<char>(version);
					}
					else
					{
						PValue = reader.ReadChar();
					}
					reader.Align();
					break;

				case PrimitiveType.SByte:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<sbyte>(version);
					}
					else
					{
						PValue = unchecked((byte)reader.ReadSByte());
					}
					reader.Align();
					break;

				case PrimitiveType.Byte:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<byte>(version);
					}
					else
					{
						PValue = reader.ReadByte();
					}
					reader.Align();
					break;

				case PrimitiveType.Short:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<short>(version);
					}
					else
					{
						PValue = unchecked((ushort)reader.ReadInt16());
					}
					reader.Align();
					break;

				case PrimitiveType.UShort:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<ushort>(version);
					}
					else
					{
						PValue = reader.ReadUInt16();
					}
					reader.Align();
					break;

				case PrimitiveType.Int:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<int>(version);
					}
					else
					{
						PValue = unchecked((uint)reader.ReadInt32());
					}
					break;

				case PrimitiveType.UInt:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<uint>(version);
					}
					else
					{
						PValue = reader.ReadUInt32();
					}
					break;

				case PrimitiveType.Long:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<long>(version);
					}
					else
					{
						PValue = unchecked((ulong)reader.ReadInt64());
					}
					break;

				case PrimitiveType.ULong:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<ulong>(version);
					}
					else
					{
						PValue = reader.ReadUInt64();
					}
					break;

				case PrimitiveType.Single:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<float>(version);
					}
					else
					{
						PValue = BitConverter.SingleToUInt32Bits(reader.ReadSingle());
					}
					break;

				case PrimitiveType.Double:
					if (etalon.IsArray)
					{
						CValue = reader.ReadPrimitiveArray<double>(version);
					}
					else
					{
						PValue = BitConverter.DoubleToUInt64Bits(reader.ReadDouble());
					}
					break;

				case PrimitiveType.String:
					if (etalon.IsArray)
					{
						CValue = reader.ReadStringArray(version);
					}
					else
					{
						CValue = reader.ReadUtf8StringAligned().String;
					}
					break;

				case PrimitiveType.Complex:
					if (etalon.IsArray)
					{
						int count = reader.ReadInt32();

						long remainingBytes = reader.Length - reader.Position;
						if (remainingBytes < count)
						{
							throw new EndOfStreamException($"When reading field {etalon.Name}, Stream only has {remainingBytes} bytes remaining, so {count} complex elements of type {etalon.Type.Name} cannot be read.");
						}

						IAsset[] structures = new IAsset[count];
						for (int i = 0; i < count; i++)
						{
							structures[i] = CreateAndReadComplexStructure(ref reader, version, flags, depth, etalon);
						}
						CValue = structures;
					}
					else
					{
						CValue = CreateAndReadComplexStructure(ref reader, version, flags, depth, etalon);
					}
					break;

				default:
					throw new NotSupportedException(etalon.Type.Type.ToString());
			}

			static IAsset CreateAndReadComplexStructure(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, SerializableType.Field etalon)
			{
				IAsset asset = etalon.Type.CreateInstance(depth + 1, version);
				if (asset is SerializableStructure structure)
				{
					structure.Read(ref reader, version, flags);
				}
				else
				{
					asset.Read(ref reader, flags);
				}

				return asset;
			}
		}

		public void Write(AssetWriter writer, in SerializableType.Field etalon)
		{
			switch (etalon.Type.Type)
			{
				case PrimitiveType.Bool:
					if (etalon.IsArray)
					{
						writer.WriteArray((bool[])CValue);
					}
					else
					{
						writer.Write(PValue != 0);
					}
					writer.AlignStream();
					break;

				case PrimitiveType.Char:
					if (etalon.IsArray)
					{
						writer.WriteArray((char[])CValue);
					}
					else
					{
						writer.Write((char)PValue);
					}
					writer.AlignStream();
					break;

				case PrimitiveType.SByte:
					if (etalon.IsArray)
					{
						writer.WriteArray((byte[])CValue);
					}
					else
					{
						writer.Write(unchecked((sbyte)PValue));
					}
					writer.AlignStream();
					break;

				case PrimitiveType.Byte:
					if (etalon.IsArray)
					{
						writer.WriteArray((byte[])CValue);
					}
					else
					{
						writer.Write((byte)PValue);
					}
					writer.AlignStream();
					break;

				case PrimitiveType.Short:
					if (etalon.IsArray)
					{
						writer.WriteArray((short[])CValue);
					}
					else
					{
						writer.Write(unchecked((short)PValue));
					}
					writer.AlignStream();
					break;

				case PrimitiveType.UShort:
					if (etalon.IsArray)
					{
						writer.WriteArray((ushort[])CValue);
					}
					else
					{
						writer.Write((ushort)PValue);
					}
					writer.AlignStream();
					break;

				case PrimitiveType.Int:
					if (etalon.IsArray)
					{
						writer.WriteArray((int[])CValue);
					}
					else
					{
						writer.Write(unchecked((int)PValue));
					}
					break;

				case PrimitiveType.UInt:
					if (etalon.IsArray)
					{
						writer.WriteArray((uint[])CValue);
					}
					else
					{
						writer.Write((uint)PValue);
					}
					break;

				case PrimitiveType.Long:
					if (etalon.IsArray)
					{
						writer.WriteArray((long[])CValue);
					}
					else
					{
						writer.Write(unchecked((long)PValue));
					}
					break;

				case PrimitiveType.ULong:
					if (etalon.IsArray)
					{
						writer.WriteArray((ulong[])CValue);
					}
					else
					{
						writer.Write(PValue);
					}
					break;

				case PrimitiveType.Single:
					if (etalon.IsArray)
					{
						writer.WriteArray((float[])CValue);
					}
					else
					{
						writer.Write(BitConverter.UInt32BitsToSingle((uint)PValue));
					}
					break;

				case PrimitiveType.Double:
					if (etalon.IsArray)
					{
						writer.WriteArray((double[])CValue);
					}
					else
					{
						writer.Write(BitConverter.UInt64BitsToDouble(PValue));
					}
					break;

				case PrimitiveType.String:
					if (etalon.IsArray)
					{
						writer.WriteArray((string[])CValue);
					}
					else
					{
						writer.Write((string)CValue);
					}
					break;

				case PrimitiveType.Complex:
					if (etalon.IsArray)
					{
						writer.WriteAssetArray((IAsset[])CValue);
					}
					else
					{
						((IAsset)CValue).Write(writer);
					}
					break;

				default:
					throw new NotSupportedException(etalon.Type.Type.ToString());
			}
		}

		public YamlNode ExportYaml(IExportContainer container, in SerializableType.Field etalon)
		{
			if (etalon.IsArray)
			{
				if (etalon.Type.Type == PrimitiveType.Complex)
				{
					IAsset[] structures = (IAsset[])CValue;
					YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
					foreach (IAsset structure in structures)
					{
						node.Add(structure.ExportYaml(container));
					}
					return node;
				}
				else
				{
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							{
								bool[] array = (bool[])CValue;
								return array.ExportYaml();
							}
						case PrimitiveType.Char:
							{
								char[] array = (char[])CValue;
								return array.ExportYaml();
							}
						case PrimitiveType.SByte:
							{
								byte[] array = (byte[])CValue;
								return array.ExportYaml();
							}
						case PrimitiveType.Byte:
							{
								byte[] array = (byte[])CValue;
								return array.ExportYaml();
							}
						case PrimitiveType.Short:
							{
								short[] array = (short[])CValue;
								return array.ExportYaml(true);
							}
						case PrimitiveType.UShort:
							{
								ushort[] array = (ushort[])CValue;
								return array.ExportYaml(true);
							}
						case PrimitiveType.Int:
							{
								int[] array = (int[])CValue;
								return array.ExportYaml(true);
							}
						case PrimitiveType.UInt:
							{
								uint[] array = (uint[])CValue;
								return array.ExportYaml(true);
							}
						case PrimitiveType.Long:
							{
								long[] array = (long[])CValue;
								return array.ExportYaml(true);
							}
						case PrimitiveType.ULong:
							{
								ulong[] array = (ulong[])CValue;
								return array.ExportYaml(true);
							}
						case PrimitiveType.Single:
							{
								float[] array = (float[])CValue;
								return array.ExportYaml();
							}
						case PrimitiveType.Double:
							{
								double[] array = (double[])CValue;
								return array.ExportYaml();
							}
						case PrimitiveType.String:
							{
								string[] array = (string[])CValue;
								return array.ExportYaml();
							}
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
				}
			}
			else
			{
				if (etalon.Type.Type == PrimitiveType.Complex)
				{
					IAsset structure = (IAsset)CValue;
					return structure.ExportYaml(container);
				}
				else
				{
					return etalon.Type.Type switch
					{
						PrimitiveType.Bool => new YamlScalarNode(PValue != 0),
						PrimitiveType.Char => new YamlScalarNode((int)(char)PValue),
						PrimitiveType.SByte => new YamlScalarNode(unchecked((sbyte)PValue)),
						PrimitiveType.Byte => new YamlScalarNode((byte)PValue),
						PrimitiveType.Short => new YamlScalarNode(unchecked((short)PValue)),
						PrimitiveType.UShort => new YamlScalarNode((ushort)PValue),
						PrimitiveType.Int => new YamlScalarNode(unchecked((int)PValue)),
						PrimitiveType.UInt => new YamlScalarNode((uint)PValue),
						PrimitiveType.Long => new YamlScalarNode(unchecked((long)PValue)),
						PrimitiveType.ULong => new YamlScalarNode(PValue),
						PrimitiveType.Single => new YamlScalarNode(BitConverter.UInt32BitsToSingle((uint)PValue)),
						PrimitiveType.Double => new YamlScalarNode(BitConverter.UInt64BitsToDouble(PValue)),
						PrimitiveType.String => new YamlScalarNode((string)CValue),
						_ => throw new NotSupportedException(etalon.Type.Type.ToString()),
					};
				}
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context, SerializableType.Field etalon)
		{
			if (etalon.Type.Type == PrimitiveType.Complex)
			{
				if (etalon.IsArray)
				{
					IAsset[] structures = (IAsset[])CValue;
					if (structures.Length > 0 && structures[0] is IDependent)
					{
						foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(structures.Cast<IDependent>(), etalon.Name))
						{
							yield return asset;
						}
					}
				}
				else
				{
					IAsset structure = (IAsset)CValue;
					if (structure is IDependent dependent)
					{
						foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(dependent, etalon.Name))
						{
							yield return asset;
						}
					}
				}
			}
		}

		private string GetDebuggerDisplay()
		{
			return CValue?.ToString() ?? PValue.ToString();
		}
	}
}
