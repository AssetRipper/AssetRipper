using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Tests;

public class SerializableValueBinaryTests
{
	private static IEnumerable<TestCaseData> PrimitiveArrays()
	{
		foreach (PrimitiveType type in new[] { PrimitiveType.Bool, PrimitiveType.Char, PrimitiveType.Byte, PrimitiveType.SByte,
			PrimitiveType.Short, PrimitiveType.UShort, PrimitiveType.Int, PrimitiveType.UInt, PrimitiveType.Long, PrimitiveType.ULong,
			PrimitiveType.Single, PrimitiveType.Double })
		{
			foreach (EndianType endian in new[] { EndianType.LittleEndian, EndianType.BigEndian })
			{
				foreach (int count in new[] { 0, 3, 5001 })
				{
					yield return new(type, endian, count);
				}
			}
		}
	}

	[TestCaseSource(nameof(PrimitiveArrays))]
	public void PrimitiveArrayPreservesEveryElement(PrimitiveType type, EndianType endian, int count)
	{
		FixtureCollection collection = new(endian);
		using MemoryStream input = new();
		using (EndianWriter writer = new(input, endian))
		{
			writer.Write(count);
			for (int i = 0; i < count; i++)
			{
				WritePrimitiveValue(writer, type, i);
			}
			writer.AlignStream();
		}
		AssertRoundTrip(input.ToArray(), collection, new(SerializablePrimitiveType.GetOrCreate(type), 1, "array", true));
	}

	private static void WritePrimitiveValue(EndianWriter writer, PrimitiveType type, int index)
	{
		switch (type)
		{
			case PrimitiveType.Bool:
				writer.Write(index % 2 == 0);
				break;
			case PrimitiveType.Char:
				writer.Write((ushort)(0x4E00 + index));
				break;
			case PrimitiveType.Byte:
				writer.Write(unchecked((byte)index));
				break;
			case PrimitiveType.SByte:
				writer.Write(unchecked((sbyte)index));
				break;
			case PrimitiveType.Short:
				writer.Write((short)(index - 2500));
				break;
			case PrimitiveType.UShort:
				writer.Write((ushort)index);
				break;
			case PrimitiveType.Int:
				writer.Write(index - 2500);
				break;
			case PrimitiveType.UInt:
				writer.Write((uint)index);
				break;
			case PrimitiveType.Long:
				writer.Write((long)index << 33);
				break;
			case PrimitiveType.ULong:
				writer.Write((ulong)index << 33);
				break;
			case PrimitiveType.Single:
				writer.Write(index + 0.125f);
				break;
			case PrimitiveType.Double:
				writer.Write(index + 0.125);
				break;
		}
	}

	[Test]
	public void StringArrayPreservesUtf8AndElementAlignment()
	{
		FixtureCollection collection = new(EndianType.LittleEndian);
		using MemoryStream input = new();
		using (EndianWriter writer = new(input, collection.EndianType))
		{
			writer.Write(3);
			foreach (string value in new[] { "a", "", "日本語" })
			{
				writer.Write((Utf8String)value);
				writer.AlignStream();
			}
		}
		AssertRoundTrip(input.ToArray(), collection, new(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 1, "strings", true));
	}

	[Test]
	public void CharacterPreservesBothBytes()
	{
		FixtureCollection collection = new(EndianType.LittleEndian);
		SerializableValue value = default;
		value.AsChar = '\u4E00';
		Assert.That(value.AsChar, Is.EqualTo('\u4E00'));
		AssertRoundTrip([0x00, 0x4E, 0x00, 0x00], collection, new(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Char), 0, "character", true));
	}

	private static void AssertRoundTrip(byte[] bytes, AssetCollection collection, SerializableType.Field field)
	{
		EndianSpanReader reader = new(bytes, collection.EndianType);
		SerializableValue value = default;
		value.Read(ref reader, collection.Version, collection.Flags, 0, field, ITypeResolver.Null);
		Assert.That(reader.Position, Is.EqualTo(bytes.Length));
		using MemoryStream output = new();
		using AssetWriter writer = new(output, collection);
		value.Write(writer, field);
		Assert.That(output.ToArray(), Is.EqualTo(bytes));
	}

	private sealed class FixtureBundle : Bundle
	{
		public override string Name => "Fixture";
	}

	private sealed class FixtureCollection : AssetCollection
	{
		public FixtureCollection(EndianType endian) : base(new FixtureBundle())
		{
			Version = UnityVersion.Parse("6000.5.10f1");
			EndianType = endian;
		}
	}
}
