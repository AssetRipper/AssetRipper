using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SerializationLogic;
using System.Collections;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Assembly.Serializable
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public record struct SerializableValue(ulong PValue, object CValue)
	{
		#region AsType Properties
		public bool AsBoolean
		{
			readonly get => PValue != 0;
			set => SetPrimitive(value ? 1U : 0U);
		}

		public char AsChar
		{
			readonly get => unchecked((char)PValue);
			set => SetPrimitive(unchecked((byte)value));
		}

		public sbyte AsSByte
		{
			readonly get => unchecked((sbyte)PValue);
			set => SetPrimitive(unchecked((byte)value));
		}

		public byte AsByte
		{
			readonly get => unchecked((byte)PValue);
			set => SetPrimitive(value);
		}

		public short AsInt16
		{
			readonly get => unchecked((short)PValue);
			set => SetPrimitive(unchecked((ushort)value));
		}

		public ushort AsUInt16
		{
			readonly get => unchecked((ushort)PValue);
			set => SetPrimitive(value);
		}

		public int AsInt32
		{
			readonly get => unchecked((int)PValue);
			set => SetPrimitive(unchecked((uint)value));
		}

		public uint AsUInt32
		{
			readonly get => unchecked((uint)PValue);
			set => SetPrimitive(value);
		}

		public long AsInt64
		{
			readonly get => unchecked((long)PValue);
			set => SetPrimitive(unchecked((ulong)value));
		}

		public ulong AsUInt64
		{
			readonly get => PValue;
			set => SetPrimitive(value);
		}

		public float AsSingle
		{
			readonly get => BitConverter.UInt32BitsToSingle(AsUInt32);
			set => AsUInt32 = BitConverter.SingleToUInt32Bits(value);
		}

		public double AsDouble
		{
			readonly get => BitConverter.UInt64BitsToDouble(AsUInt64);
			set => AsUInt64 = BitConverter.DoubleToUInt64Bits(value);
		}

		public string AsString
		{
			readonly get => CValue as string ?? "";
			set => SetReference(value);
		}

		public IUnityAssetBase AsAsset
		{
			readonly get => (IUnityAssetBase)CValue;
			set => SetReference(value);
		}

		public readonly SerializableStructure AsStructure => (SerializableStructure)CValue;

		public readonly IPPtr AsPPtr => (IPPtr)CValue;

		public SerializablePair AsPair
		{
			readonly get => (SerializablePair)CValue;
			set => SetReference(value);
		}

		public bool[] AsBooleanArray
		{
			readonly get => CValue as bool[] ?? [];
			set => SetReference(value);
		}

		public char[] AsCharArray
		{
			readonly get => CValue as char[] ?? [];
			set => SetReference(value);
		}

		public sbyte[] AsSByteArray
		{
			readonly get => CValue as sbyte[] ?? [];
			set => SetReference(value);
		}

		public byte[] AsByteArray
		{
			readonly get => CValue as byte[] ?? [];
			set => SetReference(value);
		}

		public short[] AsInt16Array
		{
			readonly get => CValue as short[] ?? [];
			set => SetReference(value);
		}

		public ushort[] AsUInt16Array
		{
			readonly get => CValue as ushort[] ?? [];
			set => SetReference(value);
		}

		public int[] AsInt32Array
		{
			readonly get => CValue as int[] ?? [];
			set => SetReference(value);
		}

		public uint[] AsUInt32Array
		{
			readonly get => CValue as uint[] ?? [];
			set => SetReference(value);
		}

		public long[] AsInt64Array
		{
			readonly get => CValue as long[] ?? [];
			set => SetReference(value);
		}

		public ulong[] AsUInt64Array
		{
			readonly get => CValue as ulong[] ?? [];
			set => SetReference(value);
		}

		public float[] AsSingleArray
		{
			readonly get => CValue as float[] ?? [];
			set => SetReference(value);
		}

		public double[] AsDoubleArray
		{
			readonly get => CValue as double[] ?? [];
			set => SetReference(value);
		}

		public string[] AsStringArray
		{
			readonly get => CValue as string[] ?? [];
			set => SetReference(value);
		}

		public IUnityAssetBase[] AsAssetArray
		{
			readonly get => CValue as IUnityAssetBase[] ?? [];
			set => SetReference(value);
		}

		public SerializablePair[] AsPairArray
		{
			readonly get => CValue as SerializablePair[] ?? [];
			set => SetReference(value);
		}

		public bool[][] AsBooleanArrayArray
		{
			readonly get => CValue as bool[][] ?? [];
			set => SetReference(value);
		}

		public char[][] AsCharArrayArray
		{
			readonly get => CValue as char[][] ?? [];
			set => SetReference(value);
		}

		public sbyte[][] AsSByteArrayArray
		{
			readonly get => CValue as sbyte[][] ?? [];
			set => SetReference(value);
		}

		public byte[][] AsByteArrayArray
		{
			readonly get => CValue as byte[][] ?? [];
			set => SetReference(value);
		}

		public short[][] AsInt16ArrayArray
		{
			readonly get => CValue as short[][] ?? [];
			set => SetReference(value);
		}

		public ushort[][] AsUInt16ArrayArray
		{
			readonly get => CValue as ushort[][] ?? [];
			set => SetReference(value);
		}

		public int[][] AsInt32ArrayArray
		{
			readonly get => CValue as int[][] ?? [];
			set => SetReference(value);
		}

		public uint[][] AsUInt32ArrayArray
		{
			readonly get => CValue as uint[][] ?? [];
			set => SetReference(value);
		}

		public long[][] AsInt64ArrayArray
		{
			readonly get => CValue as long[][] ?? [];
			set => SetReference(value);
		}

		public ulong[][] AsUInt64ArrayArray
		{
			readonly get => CValue as ulong[][] ?? [];
			set => SetReference(value);
		}

		public float[][] AsSingleArrayArray
		{
			readonly get => CValue as float[][] ?? [];
			set => SetReference(value);
		}

		public double[][] AsDoubleArrayArray
		{
			readonly get => CValue as double[][] ?? [];
			set => SetReference(value);
		}

		public string[][] AsStringArrayArray
		{
			readonly get => CValue as string[][] ?? [];
			set => SetReference(value);
		}

		public IUnityAssetBase[][] AsAssetArrayArray
		{
			readonly get => CValue as IUnityAssetBase[][] ?? [];
			set => SetReference(value);
		}

		private void SetPrimitive(ulong value)
		{
			if (CValue is not null)
			{
				throw new InvalidOperationException("Value is not a primitive type.");
			}
			else
			{
				PValue = value;
			}
		}

		private void SetReference<T>(T value) where T : class
		{
			if (CValue is not null and not T)
			{
				throw new InvalidOperationException($"Object value is not a {typeof(T).Name}, but instead {CValue.GetType().Name}.");
			}
			else if (PValue != 0)
			{
				throw new InvalidOperationException($"Primitive value is not zero, but instead {PValue}.");
			}
			else
			{
				CValue = value;
			}
		}
		#endregion

		public readonly ref SerializableValue this[string name] => ref AsStructure[name];

		public static SerializableValue FromPrimitive<T>(T value) where T : notnull
		{
			SerializableValue result = default;
			if (typeof(T) == typeof(bool))
			{
				result.AsBoolean = (bool)(object)value;
			}
			else if (typeof(T) == typeof(char))
			{
				result.AsChar = (char)(object)value;
			}
			else if (typeof(T) == typeof(sbyte))
			{
				result.AsSByte = (sbyte)(object)value;
			}
			else if (typeof(T) == typeof(byte))
			{
				result.AsByte = (byte)(object)value;
			}
			else if (typeof(T) == typeof(short))
			{
				result.AsInt16 = (short)(object)value;
			}
			else if (typeof(T) == typeof(ushort))
			{
				result.AsUInt16 = (ushort)(object)value;
			}
			else if (typeof(T) == typeof(int))
			{
				result.AsInt32 = (int)(object)value;
			}
			else if (typeof(T) == typeof(uint))
			{
				result.AsUInt32 = (uint)(object)value;
			}
			else if (typeof(T) == typeof(long))
			{
				result.AsInt64 = (long)(object)value;
			}
			else if (typeof(T) == typeof(ulong))
			{
				result.AsUInt64 = (ulong)(object)value;
			}
			else if (typeof(T) == typeof(float))
			{
				result.AsSingle = (float)(object)value;
			}
			else if (typeof(T) == typeof(double))
			{
				result.AsDouble = (double)(object)value;
			}
			else if (typeof(T) == typeof(string))
			{
				result.AsString = (string)(object)value;
			}
			else if (typeof(T) == typeof(Utf8String))
			{
				result.AsString = ((Utf8String)(object)value).String;
			}
			else
			{
				throw new NotSupportedException($"Type {typeof(T)} is not supported.");
			}
			return result;
		}

		public void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, in SerializableType.Field etalon)
		{
			switch (etalon.ArrayDepth)
			{
				case 0:
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							AsBoolean = reader.ReadBoolean();
							break;
						case PrimitiveType.Char:
							AsChar = reader.ReadChar();
							break;
						case PrimitiveType.SByte:
							AsSByte = reader.ReadSByte();
							break;
						case PrimitiveType.Byte:
							AsByte = reader.ReadByte();
							break;
						case PrimitiveType.Short:
							AsInt16 = reader.ReadInt16();
							break;
						case PrimitiveType.UShort:
							AsUInt16 = reader.ReadUInt16();
							break;
						case PrimitiveType.Int:
							AsInt32 = reader.ReadInt32();
							break;
						case PrimitiveType.UInt:
							AsUInt32 = reader.ReadUInt32();
							break;
						case PrimitiveType.Long:
							AsInt64 = reader.ReadInt64();
							break;
						case PrimitiveType.ULong:
							AsUInt64 = reader.ReadUInt64();
							break;
						case PrimitiveType.Single:
							AsSingle = reader.ReadSingle();
							break;
						case PrimitiveType.Double:
							AsDouble = reader.ReadDouble();
							break;
						case PrimitiveType.String:
							AsString = reader.ReadUtf8StringAligned().String;
							break;
						case PrimitiveType.Complex:
							AsAsset = CreateAndReadComplexStructure(ref reader, version, flags, depth, etalon);
							break;
						case PrimitiveType.Pair:
						case PrimitiveType.MapPair:
							{
								SerializablePair pair = new(etalon.Type, depth + 1);
								pair.Read(ref reader, version, flags);
								AsPair = pair;
							}
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				case 1:
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							AsBooleanArray = reader.ReadPrimitiveArray<bool>(version);
							break;
						case PrimitiveType.Char:
							AsCharArray = reader.ReadPrimitiveArray<char>(version);
							break;
						case PrimitiveType.SByte:
							AsSByteArray = reader.ReadPrimitiveArray<sbyte>(version);
							break;
						case PrimitiveType.Byte:
							AsByteArray = reader.ReadPrimitiveArray<byte>(version);
							break;
						case PrimitiveType.Short:
							AsInt16Array = reader.ReadPrimitiveArray<short>(version);
							break;
						case PrimitiveType.UShort:
							AsUInt16Array = reader.ReadPrimitiveArray<ushort>(version);
							break;
						case PrimitiveType.Int:
							AsInt32Array = reader.ReadPrimitiveArray<int>(version);
							break;
						case PrimitiveType.UInt:
							AsUInt32Array = reader.ReadPrimitiveArray<uint>(version);
							break;
						case PrimitiveType.Long:
							AsInt64Array = reader.ReadPrimitiveArray<long>(version);
							break;
						case PrimitiveType.ULong:
							AsUInt64Array = reader.ReadPrimitiveArray<ulong>(version);
							break;
						case PrimitiveType.Single:
							AsSingleArray = reader.ReadPrimitiveArray<float>(version);
							break;
						case PrimitiveType.Double:
							AsDoubleArray = reader.ReadPrimitiveArray<double>(version);
							break;
						case PrimitiveType.String:
							AsStringArray = reader.ReadStringArray(version);
							break;
						case PrimitiveType.Pair:
						case PrimitiveType.MapPair:
							{
								int count = reader.ReadInt32();

								long remainingBytes = reader.Length - reader.Position;
								if (remainingBytes < count)
								{
									throw new EndOfStreamException($"When reading field {etalon.Name}, Stream only has {remainingBytes} bytes remaining, so {count} pair elements of type {etalon.Type.Name} cannot be read.");
								}
								SerializablePair[] pairs = CreateArray<SerializablePair>(count);

								for (int i = 0; i < count; i++)
								{
									SerializablePair pair = new(etalon.Type, depth + 1);
									pair.Read(ref reader, version, flags);
									pairs[i] = pair;
								}

								AsPairArray = pairs;
							}
							break;
						case PrimitiveType.Complex:
							{
								int count = reader.ReadInt32();
								ThrowIfNotEnoughSpaceToReadArray(reader, etalon, count);

								IUnityAssetBase[] structures = CreateArray<IUnityAssetBase>(count);
								for (int i = 0; i < count; i++)
								{
									structures[i] = CreateAndReadComplexStructure(ref reader, version, flags, depth, etalon);
								}
								AsAssetArray = structures;
							}
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				case 2:
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							AsBooleanArrayArray = reader.ReadPrimitiveArrayArray<bool>(version);
							break;
						case PrimitiveType.Char:
							AsCharArrayArray = reader.ReadPrimitiveArrayArray<char>(version);
							break;
						case PrimitiveType.SByte:
							AsSByteArrayArray = reader.ReadPrimitiveArrayArray<sbyte>(version);
							break;
						case PrimitiveType.Byte:
							AsByteArrayArray = reader.ReadPrimitiveArrayArray<byte>(version);
							break;
						case PrimitiveType.Short:
							AsInt16ArrayArray = reader.ReadPrimitiveArrayArray<short>(version);
							break;
						case PrimitiveType.UShort:
							AsUInt16ArrayArray = reader.ReadPrimitiveArrayArray<ushort>(version);
							break;
						case PrimitiveType.Int:
							AsInt32ArrayArray = reader.ReadPrimitiveArrayArray<int>(version);
							break;
						case PrimitiveType.UInt:
							AsUInt32ArrayArray = reader.ReadPrimitiveArrayArray<uint>(version);
							break;
						case PrimitiveType.Long:
							AsInt64ArrayArray = reader.ReadPrimitiveArrayArray<long>(version);
							break;
						case PrimitiveType.ULong:
							AsUInt64ArrayArray = reader.ReadPrimitiveArrayArray<ulong>(version);
							break;
						case PrimitiveType.Single:
							AsSingleArrayArray = reader.ReadPrimitiveArrayArray<float>(version);
							break;
						case PrimitiveType.Double:
							AsDoubleArrayArray = reader.ReadPrimitiveArrayArray<double>(version);
							break;
						case PrimitiveType.String:
							AsStringArrayArray = reader.ReadStringArrayArray(version);
							break;
						case PrimitiveType.Complex:
							{
								int outerCount = reader.ReadInt32();
								ThrowIfNotEnoughSpaceToReadArray(reader, etalon, outerCount);
								IUnityAssetBase[][] result = CreateArray<IUnityAssetBase[]>(outerCount);

								for (int i = 0; i < outerCount; i++)
								{
									int innerCount = reader.ReadInt32();
									ThrowIfNotEnoughSpaceToReadArray(reader, etalon, innerCount);

									IUnityAssetBase[] structures = CreateArray<IUnityAssetBase>(innerCount);
									for (int j = 0; j < innerCount; j++)
									{
										structures[j] = CreateAndReadComplexStructure(ref reader, version, flags, depth, etalon);
									}
									result[i] = structures;

									if (etalon.Align)
									{
										reader.Align();
									}
								}

								AsAssetArrayArray = result;
							}
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				default:
					throw new NotSupportedException(etalon.ArrayDepth.ToString());
			}

			if (etalon.Align)
			{
				reader.Align();
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

		private static void ThrowIfNotEnoughSpaceToReadArray(EndianSpanReader reader, SerializableType.Field etalon, int count)
		{
			long remainingBytes = reader.Length - reader.Position;
			if (remainingBytes < count)
			{
				throw new EndOfStreamException($"When reading field {etalon.Name}, Stream only has {remainingBytes} bytes remaining, so {count} complex elements of type {etalon.Type.Name} cannot be read.");
			}
		}

		public readonly void Write(AssetWriter writer, in SerializableType.Field etalon)
		{
			switch (etalon.ArrayDepth)
			{
				case 0:
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							writer.Write(AsBoolean);
							break;
						case PrimitiveType.Char:
							writer.Write(AsChar);
							break;
						case PrimitiveType.SByte:
							writer.Write(AsSByte);
							break;
						case PrimitiveType.Byte:
							writer.Write(AsByte);
							break;
						case PrimitiveType.Short:
							writer.Write(AsInt16);
							break;
						case PrimitiveType.UShort:
							writer.Write(AsUInt16);
							break;
						case PrimitiveType.Int:
							writer.Write(AsInt32);
							break;
						case PrimitiveType.UInt:
							writer.Write(AsUInt32);
							break;
						case PrimitiveType.Long:
							writer.Write(AsInt64);
							break;
						case PrimitiveType.ULong:
							writer.Write(AsUInt64);
							break;
						case PrimitiveType.Single:
							writer.Write(AsSingle);
							break;
						case PrimitiveType.Double:
							writer.Write(AsDouble);
							break;
						case PrimitiveType.String:
							writer.Write(AsString);
							break;
						case PrimitiveType.Complex:
							AsAsset.Write(writer);
							break;
						case PrimitiveType.Pair:
						case PrimitiveType.MapPair:
							AsPair.Write(writer);
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				case 1:
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							writer.WriteArray(AsBooleanArray);
							break;
						case PrimitiveType.Char:
							writer.WriteArray(AsCharArray);
							break;
						case PrimitiveType.SByte:
							writer.WriteArray(AsSByteArray);
							break;
						case PrimitiveType.Byte:
							writer.WriteArray(AsByteArray);
							break;
						case PrimitiveType.Short:
							writer.WriteArray(AsInt16Array);
							break;
						case PrimitiveType.UShort:
							writer.WriteArray(AsUInt16Array);
							break;
						case PrimitiveType.Int:
							writer.WriteArray(AsInt32Array);
							break;
						case PrimitiveType.UInt:
							writer.WriteArray(AsUInt32Array);
							break;
						case PrimitiveType.Long:
							writer.WriteArray(AsInt64Array);
							break;
						case PrimitiveType.ULong:
							writer.WriteArray(AsUInt64Array);
							break;
						case PrimitiveType.Single:
							writer.WriteArray(AsSingleArray);
							break;
						case PrimitiveType.Double:
							writer.WriteArray(AsDoubleArray);
							break;
						case PrimitiveType.String:
							writer.WriteArray(AsStringArray);
							break;
						case PrimitiveType.Complex:
							writer.WriteAssetArray(AsAssetArray);
							break;
						case PrimitiveType.Pair:
						case PrimitiveType.MapPair:
							{
								SerializablePair[] pairs = AsPairArray;
								writer.Write(pairs.Length);
								foreach (SerializablePair pair in pairs)
								{
									pair.Write(writer);
								}
							}
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				case 2:
					throw new NotImplementedException();
				default:
					throw new NotSupportedException(etalon.ArrayDepth.ToString());
			}
			if (etalon.Align)
			{
				writer.AlignStream();
			}
		}

		public readonly void WalkEditor(AssetWalker walker, in SerializableType.Field etalon)
		{
			switch (etalon.ArrayDepth)
			{
				case 0:
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							walker.VisitPrimitive(AsBoolean);
							break;
						case PrimitiveType.Char:
							walker.VisitPrimitive(AsChar);
							break;
						case PrimitiveType.SByte:
							walker.VisitPrimitive(AsSByte);
							break;
						case PrimitiveType.Byte:
							walker.VisitPrimitive(AsByte);
							break;
						case PrimitiveType.Short:
							walker.VisitPrimitive(AsInt16);
							break;
						case PrimitiveType.UShort:
							walker.VisitPrimitive(AsUInt16);
							break;
						case PrimitiveType.Int:
							walker.VisitPrimitive(AsInt32);
							break;
						case PrimitiveType.UInt:
							walker.VisitPrimitive(AsUInt32);
							break;
						case PrimitiveType.Long:
							walker.VisitPrimitive(AsInt64);
							break;
						case PrimitiveType.ULong:
							walker.VisitPrimitive(AsUInt64);
							break;
						case PrimitiveType.Single:
							walker.VisitPrimitive(AsSingle);
							break;
						case PrimitiveType.Double:
							walker.VisitPrimitive(AsDouble);
							break;
						case PrimitiveType.String:
							walker.VisitPrimitive(AsString);
							break;
						case PrimitiveType.Complex:
							AsAsset.WalkEditor(walker);
							break;
						case PrimitiveType.Pair:
							AsPair.WalkEditor(walker);
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				case 1:
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							VisitPrimitiveArray(walker, AsBooleanArray);
							break;
						case PrimitiveType.Char:
							VisitPrimitiveArray(walker, AsCharArray);
							break;
						case PrimitiveType.SByte:
							VisitPrimitiveArray(walker, AsSByteArray);
							break;
						case PrimitiveType.Byte:
							VisitPrimitiveArray(walker, AsByteArray);
							break;
						case PrimitiveType.Short:
							VisitPrimitiveArray(walker, AsInt16Array);
							break;
						case PrimitiveType.UShort:
							VisitPrimitiveArray(walker, AsUInt16Array);
							break;
						case PrimitiveType.Int:
							VisitPrimitiveArray(walker, AsInt32Array);
							break;
						case PrimitiveType.UInt:
							VisitPrimitiveArray(walker, AsUInt32Array);
							break;
						case PrimitiveType.Long:
							VisitPrimitiveArray(walker, AsInt64Array);
							break;
						case PrimitiveType.ULong:
							VisitPrimitiveArray(walker, AsUInt64Array);
							break;
						case PrimitiveType.Single:
							VisitPrimitiveArray(walker, AsSingleArray);
							break;
						case PrimitiveType.Double:
							VisitPrimitiveArray(walker, AsDoubleArray);
							break;
						case PrimitiveType.String:
							VisitPrimitiveArray(walker, AsStringArray);
							break;
						case PrimitiveType.Complex:
							{
								IUnityAssetBase[] structures = AsAssetArray;
								if (walker.EnterList(structures))
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
											walker.DivideList(structures);
										}
									}
									walker.ExitList(structures);
								}
							}
							break;
						case PrimitiveType.MapPair:
							if (etalon.Type.Fields[0].Type.Type is PrimitiveType.String)
							{
								new PairCollection<string>(AsPairArray).WalkEditor(walker);
							}
							else
							{
								new PairCollection<SerializableValue>(AsPairArray).WalkEditor(walker);
							}
							break;
						case PrimitiveType.Pair:
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				case 2:
					throw new NotImplementedException();
				default:
					throw new NotSupportedException(etalon.ArrayDepth.ToString());
			}
		}

		private static void VisitPrimitiveArray<T>(AssetWalker walker, T[] array) where T : notnull
		{
			if (walker.EnterList(array))
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
						walker.DivideList(array);
					}
				}
				walker.ExitList(array);
			}
		}

		internal void CopyValues(SerializableValue source, int depth, in SerializableType.Field etalon, PPtrConverter converter)
		{
			switch (etalon.ArrayDepth)
			{
				case 0:
					if (etalon.Type.Type == PrimitiveType.Complex)
					{
						IUnityAssetBase thisStructure = etalon.Type.CreateInstance(depth + 1, converter.TargetCollection.Version);
						if (source.CValue is IUnityAssetBase sourceStructure)
						{
							thisStructure.CopyValues(sourceStructure, converter);
						}
						PValue = default;
						CValue = thisStructure;
					}
					else if (etalon.Type.Type is PrimitiveType.String)
					{
						PValue = default;
						CValue = source.CValue as string ?? "";
					}
					else if (etalon.Type.Type is PrimitiveType.MapPair or PrimitiveType.Pair)
					{
						PValue = default;
						SerializablePair thisPair = new(etalon.Type, depth + 1);
						thisPair.Initialize(converter.TargetCollection.Version);
						if (source.CValue is SerializablePair sourcePair)
						{
							thisPair.CopyValues(sourcePair, converter);
						}
						CValue = thisPair;
					}
					else
					{
						PValue = source.PValue;
						CValue = default!;
					}
					break;
				case 1:
					PValue = default;
					switch (etalon.Type.Type)
					{
						case PrimitiveType.Bool:
							{
								ReadOnlySpan<bool> span = source.CValue as bool[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Char:
							{
								ReadOnlySpan<char> span = source.CValue as char[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.SByte:
							{
								ReadOnlySpan<byte> span = source.CValue as byte[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Byte:
							{
								ReadOnlySpan<byte> span = source.CValue as byte[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Short:
							{
								ReadOnlySpan<short> span = source.CValue as short[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.UShort:
							{
								ReadOnlySpan<ushort> span = source.CValue as ushort[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Int:
							{
								ReadOnlySpan<int> span = source.CValue as int[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.UInt:
							{
								ReadOnlySpan<uint> span = source.CValue as uint[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Long:
							{
								ReadOnlySpan<long> span = source.CValue as long[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.ULong:
							{
								ReadOnlySpan<ulong> span = source.CValue as ulong[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Single:
							{
								ReadOnlySpan<float> span = source.CValue as float[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Double:
							{
								ReadOnlySpan<double> span = source.CValue as double[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.String:
							{
								ReadOnlySpan<string> span = source.CValue as string[];
								CValue = span.ToArray();
							}
							break;
						case PrimitiveType.Complex:
							{
								if (source.CValue is IUnityAssetBase[] sourceStructures)
								{
									IUnityAssetBase[] thisStructures = new IUnityAssetBase[sourceStructures.Length];
									for (int i = 0; i < sourceStructures.Length; i++)
									{
										IUnityAssetBase sourceStructure = sourceStructures[i];
										IUnityAssetBase thisStructure = etalon.Type.CreateInstance(depth + 1, converter.TargetCollection.Version);
										thisStructure.CopyValues(sourceStructure, converter);
										thisStructures[i] = thisStructure;
									}
									CValue = thisStructures;
								}
								else
								{
									CValue = Array.Empty<IUnityAssetBase>();
								}
							}
							break;
						case PrimitiveType.MapPair or PrimitiveType.Pair:
							{
								if (source.CValue is SerializablePair[] sourcePairs)
								{
									SerializablePair[] thisPairs = new SerializablePair[sourcePairs.Length];
									for (int i = 0; i < sourcePairs.Length; i++)
									{
										SerializablePair sourcePair = sourcePairs[i];
										SerializablePair thisPair = new(etalon.Type, depth + 1);
										thisPair.Initialize(converter.TargetCollection.Version);
										thisPair.CopyValues(sourcePair, converter);
										thisPairs[i] = thisPair;
									}
									CValue = thisPairs;
								}
								else
								{
									CValue = Array.Empty<SerializablePair>();
								}
							}
							break;
						default:
							throw new NotSupportedException(etalon.Type.Type.ToString());
					}
					break;
				case 2:
					throw new NotImplementedException();
				default:
					throw new NotSupportedException(etalon.ArrayDepth.ToString());
			}
		}

		internal void Initialize(UnityVersion version, int depth, in SerializableType.Field etalon)
		{
			PValue = default;
			CValue = etalon.ArrayDepth switch
			{
				0 => etalon.Type.Type switch
				{
					PrimitiveType.String => "",
					PrimitiveType.Complex => etalon.Type.CreateInstance(depth + 1, version),
					_ => default!,
				},
				1 => etalon.Type.Type switch
				{
					PrimitiveType.Bool => Array.Empty<bool>(),
					PrimitiveType.Char => Array.Empty<char>(),
					PrimitiveType.SByte => Array.Empty<sbyte>(),
					PrimitiveType.Byte => Array.Empty<byte>(),
					PrimitiveType.Short => Array.Empty<short>(),
					PrimitiveType.UShort => Array.Empty<ushort>(),
					PrimitiveType.Int => Array.Empty<int>(),
					PrimitiveType.UInt => Array.Empty<uint>(),
					PrimitiveType.Long => Array.Empty<long>(),
					PrimitiveType.ULong => Array.Empty<ulong>(),
					PrimitiveType.Single => Array.Empty<float>(),
					PrimitiveType.Double => Array.Empty<double>(),
					PrimitiveType.String => Array.Empty<string>(),
					PrimitiveType.Complex => Array.Empty<IUnityAssetBase>(),
					PrimitiveType.Pair or PrimitiveType.MapPair => Array.Empty<SerializablePair>(),
					_ => throw new NotSupportedException(etalon.Type.Type.ToString()),
				},
				2 => etalon.Type.Type switch
				{
					PrimitiveType.Bool => Array.Empty<bool[]>(),
					PrimitiveType.Char => Array.Empty<char[]>(),
					PrimitiveType.SByte => Array.Empty<sbyte[]>(),
					PrimitiveType.Byte => Array.Empty<byte[]>(),
					PrimitiveType.Short => Array.Empty<short[]>(),
					PrimitiveType.UShort => Array.Empty<ushort[]>(),
					PrimitiveType.Int => Array.Empty<int[]>(),
					PrimitiveType.UInt => Array.Empty<uint[]>(),
					PrimitiveType.Long => Array.Empty<long[]>(),
					PrimitiveType.ULong => Array.Empty<ulong[]>(),
					PrimitiveType.Single => Array.Empty<float[]>(),
					PrimitiveType.Double => Array.Empty<double[]>(),
					PrimitiveType.String => Array.Empty<string[]>(),
					PrimitiveType.Complex => Array.Empty<IUnityAssetBase[]>(),
					_ => throw new NotSupportedException(etalon.Type.Type.ToString()),
				},
				_ => throw new NotSupportedException(etalon.ArrayDepth.ToString()),
			};
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
					Debug.Assert(etalon.ArrayDepth == 0);
					IUnityAssetBase structure = (IUnityAssetBase)CValue;
					foreach ((string path, PPtr pptr) in structure.FetchDependencies())
					{
						yield return ($"{etalon.Name}.{path}", pptr);
					}
				}
			}
		}

		public void Reset()
		{
			PValue = default;
			switch (CValue)
			{
				case null:
					break;
				case string:
					CValue = "";
					break;
				case IUnityAssetBase asset:
					asset.Reset();
					break;
				case SerializablePair pair:
					pair.Reset();
					break;
				case bool[]:
					CValue = Array.Empty<bool>();
					break;
				case char[]:
					CValue = Array.Empty<char>();
					break;
				case sbyte[]:
					CValue = Array.Empty<sbyte>();
					break;
				case byte[]:
					CValue = Array.Empty<byte>();
					break;
				case short[]:
					CValue = Array.Empty<short>();
					break;
				case ushort[]:
					CValue = Array.Empty<ushort>();
					break;
				case int[]:
					CValue = Array.Empty<int>();
					break;
				case uint[]:
					CValue = Array.Empty<uint>();
					break;
				case long[]:
					CValue = Array.Empty<long>();
					break;
				case ulong[]:
					CValue = Array.Empty<ulong>();
					break;
				case Half[]:
					CValue = Array.Empty<Half>();
					break;
				case float[]:
					CValue = Array.Empty<float>();
					break;
				case double[]:
					CValue = Array.Empty<double>();
					break;
				case string[]:
					CValue = Array.Empty<string>();
					break;
				case IUnityAssetBase[]:
					CValue = Array.Empty<IUnityAssetBase>();
					break;
				case SerializablePair[]:
					CValue = Array.Empty<SerializablePair>();
					break;
				case bool[][]:
					CValue = Array.Empty<bool[]>();
					break;
				case char[][]:
					CValue = Array.Empty<char[]>();
					break;
				case sbyte[][]:
					CValue = Array.Empty<sbyte[]>();
					break;
				case byte[][]:
					CValue = Array.Empty<byte[]>();
					break;
				case short[][]:
					CValue = Array.Empty<short[]>();
					break;
				case ushort[][]:
					CValue = Array.Empty<ushort[]>();
					break;
				case int[][]:
					CValue = Array.Empty<int[]>();
					break;
				case uint[][]:
					CValue = Array.Empty<uint[]>();
					break;
				case long[][]:
					CValue = Array.Empty<long[]>();
					break;
				case ulong[][]:
					CValue = Array.Empty<ulong[]>();
					break;
				case Half[][]:
					CValue = Array.Empty<Half[]>();
					break;
				case float[][]:
					CValue = Array.Empty<float[]>();
					break;
				case double[][]:
					CValue = Array.Empty<double[]>();
					break;
				case string[][]:
					CValue = Array.Empty<string[]>();
					break;
				case IUnityAssetBase[][]:
					CValue = Array.Empty<IUnityAssetBase[]>();
					break;
			}
		}

		private readonly string GetDebuggerDisplay()
		{
			return CValue?.ToString() ?? PValue.ToString();
		}

		private static T[] CreateArray<T>(int length) => length is 0 ? [] : new T[length];

		private sealed class PairCollection<TKey>(SerializablePair[] array) : IReadOnlyCollection<KeyValuePair<TKey, SerializableValue>>
			where TKey : notnull
		{
			public int Count => array.Length;

			private static KeyValuePair<TKey, SerializableValue> Convert(SerializablePair pair)
			{
				if (typeof(TKey) == typeof(string))
				{
					return new KeyValuePair<TKey, SerializableValue>((TKey)(object)pair.First.AsString, pair.Second);
				}
				else if (typeof(TKey) == typeof(SerializableValue))
				{
					return new KeyValuePair<TKey, SerializableValue>((TKey)(object)pair.First, pair.Second);
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			public IEnumerator<KeyValuePair<TKey, SerializableValue>> GetEnumerator()
			{
				foreach (SerializablePair pair in array)
				{
					yield return Convert(pair);
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public void WalkEditor(AssetWalker walker)
			{
				if (walker.EnterDictionary(this))
				{
					int length = array.Length;
					if (length > 0)
					{
						int i = 0;
						while (true)
						{
							array[i].WalkEditor(walker);
							i++;
							if (i >= length)
							{
								break;
							}
							walker.DivideDictionary(this);
						}
					}
					walker.ExitDictionary(this);
				}
			}
		}
	}
}
