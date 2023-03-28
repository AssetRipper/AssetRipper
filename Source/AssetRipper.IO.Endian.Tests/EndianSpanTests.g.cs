// Auto-generated code. Do not modify manually.
namespace AssetRipper.IO.Endian.Tests;

public partial class EndianSpanTests
{
	[Theory]
	public void Int16Test(EndianType endianType)
	{
		byte[] data = new byte[sizeof(short)];
		short value1 = RandomData.NextInt16();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(short)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(short)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(short)));
		short value2 = reader.ReadInt16();
		Assert.That(reader.Position, Is.EqualTo(sizeof(short)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void UInt16Test(EndianType endianType)
	{
		byte[] data = new byte[sizeof(ushort)];
		ushort value1 = RandomData.NextUInt16();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(ushort)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ushort)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(ushort)));
		ushort value2 = reader.ReadUInt16();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ushort)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void Int32Test(EndianType endianType)
	{
		byte[] data = new byte[sizeof(int)];
		int value1 = RandomData.NextInt32();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(int)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(int)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(int)));
		int value2 = reader.ReadInt32();
		Assert.That(reader.Position, Is.EqualTo(sizeof(int)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void UInt32Test(EndianType endianType)
	{
		byte[] data = new byte[sizeof(uint)];
		uint value1 = RandomData.NextUInt32();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(uint)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(uint)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(uint)));
		uint value2 = reader.ReadUInt32();
		Assert.That(reader.Position, Is.EqualTo(sizeof(uint)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void Int64Test(EndianType endianType)
	{
		byte[] data = new byte[sizeof(long)];
		long value1 = RandomData.NextInt64();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(long)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(long)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(long)));
		long value2 = reader.ReadInt64();
		Assert.That(reader.Position, Is.EqualTo(sizeof(long)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void UInt64Test(EndianType endianType)
	{
		byte[] data = new byte[sizeof(ulong)];
		ulong value1 = RandomData.NextUInt64();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(ulong)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ulong)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(ulong)));
		ulong value2 = reader.ReadUInt64();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ulong)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void HalfTest(EndianType endianType)
	{
		byte[] data = new byte[sizeof(ushort)];
		Half value1 = RandomData.NextHalf();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(ushort)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ushort)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(ushort)));
		Half value2 = reader.ReadHalf();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ushort)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void SingleTest(EndianType endianType)
	{
		byte[] data = new byte[sizeof(float)];
		float value1 = RandomData.NextSingle();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(float)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(float)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(float)));
		float value2 = reader.ReadSingle();
		Assert.That(reader.Position, Is.EqualTo(sizeof(float)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void DoubleTest(EndianType endianType)
	{
		byte[] data = new byte[sizeof(double)];
		double value1 = RandomData.NextDouble();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(double)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(double)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(double)));
		double value2 = reader.ReadDouble();
		Assert.That(reader.Position, Is.EqualTo(sizeof(double)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void BooleanTest(EndianType endianType)
	{
		byte[] data = new byte[sizeof(bool)];
		bool value1 = RandomData.NextBoolean();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(bool)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(bool)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(bool)));
		bool value2 = reader.ReadBoolean();
		Assert.That(reader.Position, Is.EqualTo(sizeof(bool)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void ByteTest(EndianType endianType)
	{
		byte[] data = new byte[sizeof(byte)];
		byte value1 = RandomData.NextByte();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(byte)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(byte)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(byte)));
		byte value2 = reader.ReadByte();
		Assert.That(reader.Position, Is.EqualTo(sizeof(byte)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void SByteTest(EndianType endianType)
	{
		byte[] data = new byte[sizeof(sbyte)];
		sbyte value1 = RandomData.NextSByte();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(sbyte)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(sbyte)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(sbyte)));
		sbyte value2 = reader.ReadSByte();
		Assert.That(reader.Position, Is.EqualTo(sizeof(sbyte)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Theory]
	public void CharTest(EndianType endianType)
	{
		byte[] data = new byte[sizeof(char)];
		char value1 = RandomData.NextChar();

		EndianSpanWriter writer = new EndianSpanWriter(data, endianType);
		Assert.That(writer.Length, Is.EqualTo(sizeof(char)));
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(char)));

		EndianSpanReader reader = new EndianSpanReader(data, endianType);
		Assert.That(reader.Length, Is.EqualTo(sizeof(char)));
		char value2 = reader.ReadChar();
		Assert.That(reader.Position, Is.EqualTo(sizeof(char)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[TestCase<short>(EndianType.LittleEndian)]
	[TestCase<short>(EndianType.BigEndian)]
	[TestCase<ushort>(EndianType.LittleEndian)]
	[TestCase<ushort>(EndianType.BigEndian)]
	[TestCase<int>(EndianType.LittleEndian)]
	[TestCase<int>(EndianType.BigEndian)]
	[TestCase<uint>(EndianType.LittleEndian)]
	[TestCase<uint>(EndianType.BigEndian)]
	[TestCase<long>(EndianType.LittleEndian)]
	[TestCase<long>(EndianType.BigEndian)]
	[TestCase<ulong>(EndianType.LittleEndian)]
	[TestCase<ulong>(EndianType.BigEndian)]
	[TestCase<Half>(EndianType.LittleEndian)]
	[TestCase<Half>(EndianType.BigEndian)]
	[TestCase<float>(EndianType.LittleEndian)]
	[TestCase<float>(EndianType.BigEndian)]
	[TestCase<double>(EndianType.LittleEndian)]
	[TestCase<double>(EndianType.BigEndian)]
	[TestCase<bool>(EndianType.LittleEndian)]
	[TestCase<bool>(EndianType.BigEndian)]
	[TestCase<byte>(EndianType.LittleEndian)]
	[TestCase<byte>(EndianType.BigEndian)]
	[TestCase<sbyte>(EndianType.LittleEndian)]
	[TestCase<sbyte>(EndianType.BigEndian)]
	[TestCase<char>(EndianType.LittleEndian)]
	[TestCase<char>(EndianType.BigEndian)]
	public partial void TestGenericReadWrite<T>(EndianType endianType) where T : unmanaged;
}
