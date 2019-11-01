using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Game.Assembly
{
	public struct SerializableField
	{
		public void Read(AssetReader reader, int depth, in SerializableType.Field etalon)
		{
			switch (etalon.Type.Type)
			{
				case PrimitiveType.Bool:
					if (etalon.IsArray)
					{
						CValue = reader.ReadBooleanArray();
					}
					else
					{
						PValue = reader.ReadBoolean() ? 1U : 0U;
					}
					reader.AlignStream();
					break;

				case PrimitiveType.Char:
					if (etalon.IsArray)
					{
						CValue = reader.ReadCharArray();
					}
					else
					{
						PValue = reader.ReadChar();
					}
					reader.AlignStream();
					break;

				case PrimitiveType.SByte:
					if (etalon.IsArray)
					{
						CValue = reader.ReadByteArray();
					}
					else
					{
						PValue = unchecked((byte)reader.ReadSByte());
					}
					reader.AlignStream();
					break;

				case PrimitiveType.Byte:
					if (etalon.IsArray)
					{
						CValue = reader.ReadByteArray();
					}
					else
					{
						PValue = reader.ReadByte();
					}
					reader.AlignStream();
					break;

				case PrimitiveType.Short:
					if (etalon.IsArray)
					{
						CValue = reader.ReadInt16Array();
					}
					else
					{
						PValue = unchecked((ushort)reader.ReadInt16());
					}
					reader.AlignStream();
					break;

				case PrimitiveType.UShort:
					if (etalon.IsArray)
					{
						CValue = reader.ReadUInt16Array();
					}
					else
					{
						PValue = reader.ReadUInt16();
					}
					reader.AlignStream();
					break;

				case PrimitiveType.Int:
					if (etalon.IsArray)
					{
						CValue = reader.ReadInt32Array();
					}
					else
					{
						PValue = unchecked((uint)reader.ReadInt32());
					}
					break;

				case PrimitiveType.UInt:
					if (etalon.IsArray)
					{
						CValue = reader.ReadUInt32Array();
					}
					else
					{
						PValue = reader.ReadUInt32();
					}
					break;

				case PrimitiveType.Long:
					if (etalon.IsArray)
					{
						CValue = reader.ReadInt64Array();
					}
					else
					{
						PValue = unchecked((ulong)reader.ReadInt64());
					}
					break;

				case PrimitiveType.ULong:
					if (etalon.IsArray)
					{
						CValue = reader.ReadUInt64Array();
					}
					else
					{
						PValue = reader.ReadUInt64();
					}
					break;

				case PrimitiveType.Single:
					if (etalon.IsArray)
					{
						CValue = reader.ReadSingleArray();
					}
					else
					{
						PValue = BitConverterExtensions.ToUInt32(reader.ReadSingle());
					}
					break;

				case PrimitiveType.Double:
					if (etalon.IsArray)
					{
						CValue = reader.ReadDoubleArray();
					}
					else
					{
						PValue = BitConverterExtensions.ToUInt64(reader.ReadDouble());
					}
					break;

				case PrimitiveType.String:
					if (etalon.IsArray)
					{
						CValue = reader.ReadStringArray();
					}
					else
					{
						CValue = reader.ReadString();
					}
					break;

				case PrimitiveType.Complex:
					if (etalon.IsArray)
					{
						int count = reader.ReadInt32();
						IAsset[] structures = new IAsset[count];
						for (int i = 0; i < count; i++)
						{
							IAsset structure = etalon.Type.CreateInstance(depth + 1);
							structure.Read(reader);
							structures[i] = structure;
						}
						CValue = structures;
					}
					else
					{
						IAsset structure = etalon.Type.CreateInstance(depth + 1);
						structure.Read(reader);
						CValue = structure;
					}
					break;

				default:
					throw new NotSupportedException(etalon.Type.Type.ToString());
			}
		}

		public void Write(AssetWriter writer, in SerializableType.Field etalon)
		{
			switch (etalon.Type.Type)
			{
				case PrimitiveType.Bool:
					if (etalon.IsArray)
					{
						((bool[])CValue).Write(writer);
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
						((char[])CValue).Write(writer);
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
						((byte[])CValue).Write(writer);
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
						((byte[])CValue).Write(writer);
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
						((short[])CValue).Write(writer);
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
						((ushort[])CValue).Write(writer);
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
						((int[])CValue).Write(writer);
					}
					else
					{
						writer.Write(unchecked((int)PValue));
					}
					break;

				case PrimitiveType.UInt:
					if (etalon.IsArray)
					{
						((uint[])CValue).Write(writer);
					}
					else
					{
						writer.Write((uint)PValue);
					}
					break;

				case PrimitiveType.Long:
					if (etalon.IsArray)
					{
						((long[])CValue).Write(writer);
					}
					else
					{
						writer.Write(unchecked((long)PValue));
					}
					break;

				case PrimitiveType.ULong:
					if (etalon.IsArray)
					{
						((ulong[])CValue).Write(writer);
					}
					else
					{
						writer.Write(PValue);
					}
					break;

				case PrimitiveType.Single:
					if (etalon.IsArray)
					{
						((float[])CValue).Write(writer);
					}
					else
					{
						writer.Write(BitConverterExtensions.ToSingle((uint)PValue));
					}
					break;

				case PrimitiveType.Double:
					if (etalon.IsArray)
					{
						((double[])CValue).Write(writer);
					}
					else
					{
						writer.Write(BitConverterExtensions.ToDouble(PValue));
					}
					break;

				case PrimitiveType.String:
					if (etalon.IsArray)
					{
						((string[])CValue).Write(writer);
					}
					else
					{
						writer.Write((string)CValue);
					}
					break;

				case PrimitiveType.Complex:
					if (etalon.IsArray)
					{
						((IAsset[])CValue).Write(writer);
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

		public YAMLNode ExportYAML(IExportContainer container, in SerializableType.Field etalon)
		{
			if (etalon.IsArray)
			{
				if (etalon.Type.Type == PrimitiveType.Complex)
				{
					IAsset[] structures = (IAsset[])CValue;
					return structures.ExportYAML(container);
				}
				else
				{
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							{
								bool[] array = (bool[])CValue;
								return array.ExportYAML();
							}
						case PrimitiveType.Char:
							{
								char[] array = (char[])CValue;
								return array.ExportYAML();
							}
						case PrimitiveType.SByte:
							{
								byte[] array = (byte[])CValue;
								return array.ExportYAML();
							}
						case PrimitiveType.Byte:
							{
								byte[] array = (byte[])CValue;
								return array.ExportYAML();
							}
						case PrimitiveType.Short:
							{
								short[] array = (short[])CValue;
								return array.ExportYAML(true);
							}
						case PrimitiveType.UShort:
							{
								ushort[] array = (ushort[])CValue;
								return array.ExportYAML(true);
							}
						case PrimitiveType.Int:
							{
								int[] array = (int[])CValue;
								return array.ExportYAML(true);
							}
						case PrimitiveType.UInt:
							{
								uint[] array = (uint[])CValue;
								return array.ExportYAML(true);
							}
						case PrimitiveType.Long:
							{
								long[] array = (long[])CValue;
								return array.ExportYAML(true);
							}
						case PrimitiveType.ULong:
							{
								ulong[] array = (ulong[])CValue;
								return array.ExportYAML(true);
							}
						case PrimitiveType.Single:
							{
								float[] array = (float[])CValue;
								return array.ExportYAML();
							}
						case PrimitiveType.Double:
							{
								double[] array = (double[])CValue;
								return array.ExportYAML();
							}
						case PrimitiveType.String:
							{
								string[] array = (string[])CValue;
								return array.ExportYAML();
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
					return structure.ExportYAML(container);
				}
				else
				{
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							return new YAMLScalarNode(PValue != 0);
						case PrimitiveType.Char:
							return new YAMLScalarNode((int)(char)PValue);
						case PrimitiveType.SByte:
							return new YAMLScalarNode(unchecked((sbyte)PValue));
						case PrimitiveType.Byte:
							return new YAMLScalarNode((byte)PValue);
						case PrimitiveType.Short:
							return new YAMLScalarNode(unchecked((short)PValue));
						case PrimitiveType.UShort:
							return new YAMLScalarNode((ushort)PValue);
						case PrimitiveType.Int:
							return new YAMLScalarNode(unchecked((int)PValue));
						case PrimitiveType.UInt:
							return new YAMLScalarNode((uint)PValue);
						case PrimitiveType.Long:
							return new YAMLScalarNode(unchecked((long)PValue));
						case PrimitiveType.ULong:
							return new YAMLScalarNode(PValue);
						case PrimitiveType.Single:
							return new YAMLScalarNode(BitConverterExtensions.ToSingle((uint)PValue));
						case PrimitiveType.Double:
							return new YAMLScalarNode(BitConverterExtensions.ToDouble(PValue));
						case PrimitiveType.String:
							return new YAMLScalarNode((string)CValue);
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
				}
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context, SerializableType.Field etalon)
		{
			if (etalon.Type.Type == PrimitiveType.Complex)
			{
				if (etalon.IsArray)
				{
					IAsset[] structures = (IAsset[])CValue;
					if (structures.Length > 0 && structures[0] is IDependent)
					{
						foreach (PPtr<Object> asset in context.FetchDependencies(structures.Cast<IDependent>(), etalon.Name))
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
						foreach (PPtr<Object> asset in context.FetchDependencies(dependent, etalon.Name))
						{
							yield return asset;
						}
					}
				}
			}
		}

		public ulong PValue { get; set; }
		public object CValue { get; set; }
	}
}
