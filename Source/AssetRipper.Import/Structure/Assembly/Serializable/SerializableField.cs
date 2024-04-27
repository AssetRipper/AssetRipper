using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
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

						IUnityAssetBase[] structures = new IUnityAssetBase[count];
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

			static IUnityAssetBase CreateAndReadComplexStructure(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, SerializableType.Field etalon)
			{
				IUnityAssetBase asset = etalon.Type.CreateInstance(depth + 1, version);
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

		public readonly void Write(AssetWriter writer, in SerializableType.Field etalon)
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
						writer.WriteAssetArray((IUnityAssetBase[])CValue);
					}
					else
					{
						((IUnityAssetBase)CValue).Write(writer);
					}
					break;

				default:
					throw new NotSupportedException(etalon.Type.Type.ToString());
			}
		}

		public readonly void WalkEditor(AssetWalker walker, in SerializableType.Field etalon)
		{
			if (etalon.IsArray)
			{
				if (etalon.Type.Type == PrimitiveType.Complex)
				{
					IUnityAssetBase[] structures = (IUnityAssetBase[])CValue;
					if (walker.EnterArray(structures))
					{
						int length = structures.Length;
						if (length > 0)
						{
							int i = 0;
							while (true)
							{
								structures[i].WalkEditor(walker);
								i++;
								if (i >= length)
								{
									break;
								}
								walker.DivideArray(structures);
							}
						}
						walker.ExitArray(structures);
					}
				}
				else
				{
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							{
								bool[] array = (bool[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.Char:
							{
								char[] array = (char[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.SByte:
							{
								byte[] array = (byte[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.Byte:
							{
								byte[] array = (byte[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.Short:
							{
								short[] array = (short[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.UShort:
							{
								ushort[] array = (ushort[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.Int:
							{
								int[] array = (int[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.UInt:
							{
								uint[] array = (uint[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.Long:
							{
								long[] array = (long[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.ULong:
							{
								ulong[] array = (ulong[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.Single:
							{
								float[] array = (float[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.Double:
							{
								double[] array = (double[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
							}
						case PrimitiveType.String:
							{
								string[] array = (string[])CValue;
								VisitPrimitiveArray(walker, array);
								break;
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
					IUnityAssetBase structure = (IUnityAssetBase)CValue;
					structure.WalkEditor(walker);
				}
				else
				{
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							walker.VisitPrimitive(PValue != 0);
							break;
						case PrimitiveType.Char:
							walker.VisitPrimitive((char)PValue);
							break;
						case PrimitiveType.SByte:
							walker.VisitPrimitive(unchecked((sbyte)PValue));
							break;
						case PrimitiveType.Byte:
							walker.VisitPrimitive((byte)PValue);
							break;
						case PrimitiveType.Short:
							walker.VisitPrimitive(unchecked((short)PValue));
							break;
						case PrimitiveType.UShort:
							walker.VisitPrimitive((ushort)PValue);
							break;
						case PrimitiveType.Int:
							walker.VisitPrimitive(unchecked((int)PValue));
							break;
						case PrimitiveType.UInt:
							walker.VisitPrimitive((uint)PValue);
							break;
						case PrimitiveType.Long:
							walker.VisitPrimitive(unchecked((long)PValue));
							break;
						case PrimitiveType.ULong:
							walker.VisitPrimitive(PValue);
							break;
						case PrimitiveType.Single:
							walker.VisitPrimitive(BitConverter.UInt32BitsToSingle((uint)PValue));
							break;
						case PrimitiveType.Double:
							walker.VisitPrimitive(BitConverter.UInt64BitsToDouble(PValue));
							break;
						case PrimitiveType.String:
							walker.VisitPrimitive((string)CValue);
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
				}
			}
		}

		private static void VisitPrimitiveArray<T>(AssetWalker walker, T[] array)
		{
			if (walker.EnterArray(array))
			{
				int length = array.Length;
				if (length > 0)
				{
					int i = 0;
					while (true)
					{
						walker.VisitPrimitive(array[i]);
						i++;
						if (i >= length)
						{
							break;
						}
						walker.DivideArray(array);
					}
				}
				walker.ExitArray(array);
			}
		}

		internal void CopyValues(SerializableField source, UnityVersion version, int depth, in SerializableType.Field etalon, PPtrConverter converter)
		{
			if (etalon.IsArray)
			{
				PValue = default;
				if (etalon.Type.Type == PrimitiveType.Complex)
				{
					IUnityAssetBase[] sourceStructures = (IUnityAssetBase[])source.CValue;
					IUnityAssetBase[] thisStructures = new IUnityAssetBase[sourceStructures.Length];
					for (int i = 0; i < sourceStructures.Length; i++)
					{
						IUnityAssetBase sourceStructure = sourceStructures[i];
						IUnityAssetBase thisStructure = etalon.Type.CreateInstance(depth + 1, version);
						thisStructure.CopyValues(sourceStructure, converter);
						thisStructures[i] = thisStructure;
					}
					CValue = thisStructures;
				}
				else
				{
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							{
								ReadOnlySpan<bool> span = (bool[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Char:
							{
								ReadOnlySpan<char> span = (char[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.SByte:
							{
								ReadOnlySpan<byte> span = (byte[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Byte:
							{
								ReadOnlySpan<byte> span = (byte[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Short:
							{
								ReadOnlySpan<short> span = (short[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.UShort:
							{
								ReadOnlySpan<ushort> span = (ushort[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Int:
							{
								ReadOnlySpan<int> span = (int[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.UInt:
							{
								ReadOnlySpan<uint> span = (uint[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Long:
							{
								ReadOnlySpan<long> span = (long[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.ULong:
							{
								ReadOnlySpan<ulong> span = (ulong[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Single:
							{
								ReadOnlySpan<float> span = (float[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Double:
							{
								ReadOnlySpan<double> span = (double[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.String:
							{
								ReadOnlySpan<string> span = (string[])source.CValue;
								CValue = span.ToArray();
							}
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
				}
			}
			else if (etalon.Type.Type == PrimitiveType.Complex)
			{
				IUnityAssetBase sourceStructure = (IUnityAssetBase)source.CValue;
				IUnityAssetBase thisStructure = etalon.Type.CreateInstance(depth + 1, version);
				thisStructure.CopyValues(sourceStructure, converter);
				PValue = default;
				CValue = thisStructure;
			}
			else if (etalon.Type.Type is PrimitiveType.String)
			{
				string value = (string)source.CValue;
				PValue = default;
				CValue = value;
			}
			else
			{
				PValue = source.PValue;
				CValue = default!;
			}
		}

		public readonly IEnumerable<(string, PPtr)> FetchDependencies(SerializableType.Field etalon)
		{
			if (etalon.Type.Type == PrimitiveType.Complex)
			{
				if (etalon.IsArray)
				{
					IUnityAssetBase[] structures = (IUnityAssetBase[])CValue;
					for (int i = 0; i < structures.Length; i++)
					{
						foreach ((string path, PPtr pptr) in structures[i].FetchDependencies())
						{
							yield return ($"{etalon.Name}[{i}].{path}", pptr);
						}
					}
				}
				else
				{
					IUnityAssetBase structure = (IUnityAssetBase)CValue;
					foreach ((string path, PPtr pptr) in structure.FetchDependencies())
					{
						yield return ($"{etalon.Name}.{path}", pptr);
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
