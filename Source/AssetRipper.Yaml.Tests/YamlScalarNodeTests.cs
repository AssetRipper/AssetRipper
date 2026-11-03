using System.Globalization;
using System.Numerics;

namespace AssetRipper.Yaml.Tests;

public class YamlScalarNodeTests
{
	[Test]
	public async Task NullCharacterIsDoubleQuotedAndEscaped()
	{
		YamlScalarNode node = YamlScalarNode.Create("\0");
		await Assert.That(node.Value).IsEqualTo("\0");
		await Assert.That(node.NodeType).IsEqualTo(YamlNodeType.Scalar);
		await Assert.That(node.Style).IsEqualTo(ScalarStyle.DoubleQuoted);
		await Assert.That(node.EmitToString()).IsEqualTo("\"\\u0000\"");
	}

	[Test]
	public async Task EndOfTextCharacterCausesDoubleQuoting()
	{
		const string someText = "Some text\u0003";
		YamlScalarNode node = YamlScalarNode.Create(someText);
		await Assert.That(node.Value).IsEqualTo(someText);
		await Assert.That(node.NodeType).IsEqualTo(YamlNodeType.Scalar);
		await Assert.That(node.Style).IsEqualTo(ScalarStyle.DoubleQuoted);
		await Assert.That(node.EmitToString()).IsEqualTo("\"Some text\\u0003\"");
	}

	[Test]
	public async Task AsciiCharactersUsePlainStyle()
	{
		const string asciiCharacters = "Ascii Characters";
		YamlScalarNode node = YamlScalarNode.Create(asciiCharacters);
		await Assert.That(node.Value).IsEqualTo(asciiCharacters);
		await Assert.That(node.NodeType).IsEqualTo(YamlNodeType.Scalar);
		await Assert.That(node.Style).IsEqualTo(ScalarStyle.Plain);
		await Assert.That(node.EmitToString()).IsEqualTo(asciiCharacters);
	}

	[Test]
	public async Task HashAfterWhitespaceIsQuoted()
	{
		YamlScalarNode node = YamlScalarNode.Create("a #b");

		await Assert.That(node.EmitToString()).IsEqualTo("'a #b'");
	}

	[Test]
	public async Task FloatUsesInvariantDecimalSeparator()
	{
		CultureInfo previousCulture = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
			await Assert.That(YamlScalarNode.Create(1.5f).EmitToString()).IsEqualTo("1.5");
			await Assert.That(YamlScalarNode.Create(1.5d).EmitToString()).IsEqualTo("1.5");
		}
		finally
		{
			CultureInfo.CurrentCulture = previousCulture;
		}
	}

	[Test]
	public async Task ByteListTest() => await NumericListTest<byte>([0x01, 0x02, 0x03], "010203");

	[Test]
	public async Task UInt16ListTest() => await NumericListTest<ushort>([0x0102, 0x0304, 0x0506], "020104030605");

	[Test]
	public async Task UInt32ListTest() => await NumericListTest<uint>([0x01020304, 0x05060708], "0403020108070605");

	[Test]
	public async Task Int32ListTest() => await NumericListTest<int>([-1, unchecked((int)0xB0000000), int.MinValue], "ffffffff000000b000000080");

	[Test]
	public async Task UInt64ListTest() => await NumericListTest<ulong>([0x0102030405060708, 0x090A0B0C0D0E0F10], "0807060504030201100f0e0d0c0b0a09");

	[Test]
	public async Task Int64ListTest() => await NumericListTest<long>([-1L, long.MinValue], "ffffffffffffffff0000000000000080");

	private static async Task NumericListTest<T>(IReadOnlyList<T> list, string expected) where T : unmanaged, INumber<T>
	{
		YamlScalarNode node = YamlScalarNode.CreateHex(list);
		await Assert.That(node.Value).IsEqualTo(expected);
		await Assert.That(node.NodeType).IsEqualTo(YamlNodeType.Scalar);
		await Assert.That(node.Style).IsEqualTo(ScalarStyle.Plain);
		await Assert.That(node.EmitToString()).IsEqualTo(expected);
	}
}
