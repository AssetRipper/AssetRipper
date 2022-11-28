// Auto-generated code. Do not modify manually.
namespace AssetRipper.IO.Endian.Tests;

public partial class EndianSpanTests
{
	[Test]
	public void Int16LittleEndian()
	{
		byte[] data = new byte[sizeof(short)];
		short value1 = RandomData.NextInt16();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(short)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		short value2 = reader.ReadInt16();
		Assert.That(reader.Position, Is.EqualTo(sizeof(short)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void Int16BigEndian()
	{
		byte[] data = new byte[sizeof(short)];
		short value1 = RandomData.NextInt16();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(short)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		short value2 = reader.ReadInt16();
		Assert.That(reader.Position, Is.EqualTo(sizeof(short)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void UInt16LittleEndian()
	{
		byte[] data = new byte[sizeof(ushort)];
		ushort value1 = RandomData.NextUInt16();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ushort)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		ushort value2 = reader.ReadUInt16();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ushort)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void UInt16BigEndian()
	{
		byte[] data = new byte[sizeof(ushort)];
		ushort value1 = RandomData.NextUInt16();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ushort)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		ushort value2 = reader.ReadUInt16();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ushort)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void Int32LittleEndian()
	{
		byte[] data = new byte[sizeof(int)];
		int value1 = RandomData.NextInt32();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(int)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		int value2 = reader.ReadInt32();
		Assert.That(reader.Position, Is.EqualTo(sizeof(int)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void Int32BigEndian()
	{
		byte[] data = new byte[sizeof(int)];
		int value1 = RandomData.NextInt32();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(int)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		int value2 = reader.ReadInt32();
		Assert.That(reader.Position, Is.EqualTo(sizeof(int)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void UInt32LittleEndian()
	{
		byte[] data = new byte[sizeof(uint)];
		uint value1 = RandomData.NextUInt32();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(uint)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		uint value2 = reader.ReadUInt32();
		Assert.That(reader.Position, Is.EqualTo(sizeof(uint)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void UInt32BigEndian()
	{
		byte[] data = new byte[sizeof(uint)];
		uint value1 = RandomData.NextUInt32();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(uint)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		uint value2 = reader.ReadUInt32();
		Assert.That(reader.Position, Is.EqualTo(sizeof(uint)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void Int64LittleEndian()
	{
		byte[] data = new byte[sizeof(long)];
		long value1 = RandomData.NextInt64();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(long)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		long value2 = reader.ReadInt64();
		Assert.That(reader.Position, Is.EqualTo(sizeof(long)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void Int64BigEndian()
	{
		byte[] data = new byte[sizeof(long)];
		long value1 = RandomData.NextInt64();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(long)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		long value2 = reader.ReadInt64();
		Assert.That(reader.Position, Is.EqualTo(sizeof(long)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void UInt64LittleEndian()
	{
		byte[] data = new byte[sizeof(ulong)];
		ulong value1 = RandomData.NextUInt64();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ulong)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		ulong value2 = reader.ReadUInt64();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ulong)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void UInt64BigEndian()
	{
		byte[] data = new byte[sizeof(ulong)];
		ulong value1 = RandomData.NextUInt64();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ulong)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		ulong value2 = reader.ReadUInt64();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ulong)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void HalfLittleEndian()
	{
		byte[] data = new byte[sizeof(ushort)];
		Half value1 = RandomData.NextHalf();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ushort)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		Half value2 = reader.ReadHalf();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ushort)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void HalfBigEndian()
	{
		byte[] data = new byte[sizeof(ushort)];
		Half value1 = RandomData.NextHalf();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(ushort)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		Half value2 = reader.ReadHalf();
		Assert.That(reader.Position, Is.EqualTo(sizeof(ushort)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void SingleLittleEndian()
	{
		byte[] data = new byte[sizeof(float)];
		float value1 = RandomData.NextSingle();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(float)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		float value2 = reader.ReadSingle();
		Assert.That(reader.Position, Is.EqualTo(sizeof(float)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void SingleBigEndian()
	{
		byte[] data = new byte[sizeof(float)];
		float value1 = RandomData.NextSingle();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(float)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		float value2 = reader.ReadSingle();
		Assert.That(reader.Position, Is.EqualTo(sizeof(float)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void DoubleLittleEndian()
	{
		byte[] data = new byte[sizeof(double)];
		double value1 = RandomData.NextDouble();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(double)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		double value2 = reader.ReadDouble();
		Assert.That(reader.Position, Is.EqualTo(sizeof(double)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void DoubleBigEndian()
	{
		byte[] data = new byte[sizeof(double)];
		double value1 = RandomData.NextDouble();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(double)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		double value2 = reader.ReadDouble();
		Assert.That(reader.Position, Is.EqualTo(sizeof(double)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void BooleanLittleEndian()
	{
		byte[] data = new byte[sizeof(bool)];
		bool value1 = RandomData.NextBoolean();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(bool)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		bool value2 = reader.ReadBoolean();
		Assert.That(reader.Position, Is.EqualTo(sizeof(bool)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void BooleanBigEndian()
	{
		byte[] data = new byte[sizeof(bool)];
		bool value1 = RandomData.NextBoolean();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(bool)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		bool value2 = reader.ReadBoolean();
		Assert.That(reader.Position, Is.EqualTo(sizeof(bool)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void ByteLittleEndian()
	{
		byte[] data = new byte[sizeof(byte)];
		byte value1 = RandomData.NextByte();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(byte)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		byte value2 = reader.ReadByte();
		Assert.That(reader.Position, Is.EqualTo(sizeof(byte)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void ByteBigEndian()
	{
		byte[] data = new byte[sizeof(byte)];
		byte value1 = RandomData.NextByte();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(byte)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		byte value2 = reader.ReadByte();
		Assert.That(reader.Position, Is.EqualTo(sizeof(byte)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void SByteLittleEndian()
	{
		byte[] data = new byte[sizeof(sbyte)];
		sbyte value1 = RandomData.NextSByte();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(sbyte)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		sbyte value2 = reader.ReadSByte();
		Assert.That(reader.Position, Is.EqualTo(sizeof(sbyte)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void SByteBigEndian()
	{
		byte[] data = new byte[sizeof(sbyte)];
		sbyte value1 = RandomData.NextSByte();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(sbyte)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		sbyte value2 = reader.ReadSByte();
		Assert.That(reader.Position, Is.EqualTo(sizeof(sbyte)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void CharLittleEndian()
	{
		byte[] data = new byte[sizeof(char)];
		char value1 = RandomData.NextChar();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.LittleEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(char)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.LittleEndian);
		char value2 = reader.ReadChar();
		Assert.That(reader.Position, Is.EqualTo(sizeof(char)));
		Assert.That(value2, Is.EqualTo(value1));
	}

	[Test]
	public void CharBigEndian()
	{
		byte[] data = new byte[sizeof(char)];
		char value1 = RandomData.NextChar();

		EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
		writer.Write(value1);
		Assert.That(writer.Position, Is.EqualTo(sizeof(char)));

		EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
		char value2 = reader.ReadChar();
		Assert.That(reader.Position, Is.EqualTo(sizeof(char)));
		Assert.That(value2, Is.EqualTo(value1));
	}
}
