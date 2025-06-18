using AssetRipper.IO.Endian;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

internal static class EndianSpanReaderExtensions
{
	public static T[] ReadPrimitiveArray<T>(this ref EndianSpanReader reader, UnityVersion version) where T : unmanaged
	{
		int count = reader.ReadInt32();
		int index = 0;
		ThrowIfNegativeCount(count);
		ThrowIfNotEnoughSpaceForArray(ref reader, count, Unsafe.SizeOf<T>());
		T[] array = count == 0 ? [] : new T[count];
		while (index < count)
		{
			try
			{
				array[index] = reader.ReadPrimitive<T>();
			}
			catch (Exception ex)
			{
				throw new EndOfStreamException($"End of stream. Read {index}, expected {count} elements", ex);
			}
			index++;
		}
		if (IsAlignArrays(version))
		{
			reader.Align();
		}
		return array;
	}

	public static T[][] ReadPrimitiveArrayArray<T>(this ref EndianSpanReader reader, UnityVersion version) where T : unmanaged
	{
		int count = reader.ReadInt32();
		int index = 0;
		ThrowIfNegativeCount(count);
		ThrowIfNotEnoughSpaceForArray(ref reader, count, sizeof(int));
		T[][] array = count == 0 ? [] : new T[count][];
		while (index < count)
		{
			try
			{
				array[index] = reader.ReadPrimitiveArray<T>(version);
			}
			catch (Exception ex)
			{
				throw new EndOfStreamException($"End of stream. Read {index}, expected {count} elements", ex);
			}
			index++;
		}
		if (IsAlignArrays(version))
		{
			reader.Align();
		}
		return array;
	}

	public static Utf8String ReadUtf8StringAligned(this ref EndianSpanReader reader)
	{
		Utf8String result = reader.ReadUtf8String();
		reader.Align();//Alignment after strings has happened since 2.1.0
		return result;
	}

	public static string[] ReadStringArray(this ref EndianSpanReader reader, UnityVersion version)
	{
		int count = reader.ReadInt32();
		int index = 0;
		ThrowIfNegativeCount(count);
		ThrowIfNotEnoughSpaceForArray(ref reader, count, sizeof(int));
		string[] array = count == 0 ? [] : new string[count];
		while (index < count)
		{
			try
			{
				array[index] = reader.ReadUtf8StringAligned();
			}
			catch (Exception ex)
			{
				throw new EndOfStreamException($"End of stream. Read {index}, expected {count} elements", ex);
			}
			index++;
		}
		if (IsAlignArrays(version))
		{
			reader.Align();
		}
		return array;
	}

	public static string[][] ReadStringArrayArray(this ref EndianSpanReader reader, UnityVersion version)
	{
		int count = reader.ReadInt32();
		int index = 0;
		ThrowIfNegativeCount(count);
		ThrowIfNotEnoughSpaceForArray(ref reader, count, sizeof(int));
		string[][] array = count == 0 ? [] : new string[count][];
		while (index < count)
		{
			try
			{
				array[index] = reader.ReadStringArray(version);
			}
			catch (Exception ex)
			{
				throw new EndOfStreamException($"End of stream. Read {index}, expected {count} elements", ex);
			}
			index++;
		}
		if (IsAlignArrays(version))
		{
			reader.Align();
		}
		return array;
	}

	private static bool IsAlignArrays(UnityVersion version) => version.GreaterThanOrEquals(2017);

	[DebuggerHidden]
	private static void ThrowIfNegativeCount(int count)
	{
		if (count < 0)
		{
			throw new InvalidDataException($"Count cannot be negative: {count}");
		}
	}

	[DebuggerHidden]
	private static void ThrowIfNotEnoughSpaceForArray(ref EndianSpanReader reader, int elementNumberToRead, int elementSize)
	{
		int remainingBytes = reader.Length - reader.Position;
		if (remainingBytes < (long)elementNumberToRead * elementSize)
		{
			throw new EndOfStreamException($"Stream only has {remainingBytes} bytes in the stream, so {elementNumberToRead} elements of size {elementSize} cannot be read.");
		}
	}
}
