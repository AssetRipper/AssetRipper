using NUnit.Framework;
using System.Numerics;

namespace AssetRipper.Yaml.Tests;

public class YamlScalarNodeTests
{
	[Test]
	public void NullCharacterIsDoubleQuotedAndEscaped()
	{
		YamlScalarNode node = YamlScalarNode.Create("\0");
		using (Assert.EnterMultipleScope())
		{
			Assert.That(node.Value, Is.EqualTo("\0"));
			Assert.That(node.NodeType, Is.EqualTo(YamlNodeType.Scalar));
			Assert.That(node.Style, Is.EqualTo(ScalarStyle.DoubleQuoted));
			Assert.That(node.EmitToString(), Is.EqualTo("\"\\u0000\""));
		}
	}

	[Test]
	public void EndOfTextCharacterCausesDoubleQuoting()
	{
		const string someText = "Some text\u0003";
		YamlScalarNode node = YamlScalarNode.Create(someText);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(node.Value, Is.EqualTo(someText));
			Assert.That(node.NodeType, Is.EqualTo(YamlNodeType.Scalar));
			Assert.That(node.Style, Is.EqualTo(ScalarStyle.DoubleQuoted));
			Assert.That(node.EmitToString(), Is.EqualTo("\"Some text\\u0003\""));
		}
	}

	[Test]
	public void AsciiCharactersUsePlainStyle()
	{
		const string asciiCharacters = "Ascii Characters";
		YamlScalarNode node = YamlScalarNode.Create(asciiCharacters);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(node.Value, Is.EqualTo(asciiCharacters));
			Assert.That(node.NodeType, Is.EqualTo(YamlNodeType.Scalar));
			Assert.That(node.Style, Is.EqualTo(ScalarStyle.Plain));
			Assert.That(node.EmitToString(), Is.EqualTo(asciiCharacters));
		}
	}

	[Test]
	public void ByteListTest() => NumericListTest<byte>([ 0x01, 0x02, 0x03 ], "010203");

	[Test]
	public void UInt16ListTest() => NumericListTest<ushort>([ 0x0102, 0x0304, 0x0506 ], "020104030605");

	[Test]
	public void UInt32ListTest() => NumericListTest<uint>([ 0x01020304, 0x05060708 ], "0403020108070605");

	[Test]
	public void UInt64ListTest() => NumericListTest<ulong>([ 0x0102030405060708, 0x090A0B0C0D0E0F10 ], "0807060504030201100f0e0d0c0b0a09");

	private static void NumericListTest<T>(IReadOnlyList<T> list, string expected) where T : unmanaged, INumber<T>
	{
		YamlScalarNode node = YamlScalarNode.CreateHex(list);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(node.Value, Is.EqualTo(expected));
			Assert.That(node.NodeType, Is.EqualTo(YamlNodeType.Scalar));
			Assert.That(node.Style, Is.EqualTo(ScalarStyle.Plain));
			Assert.That(node.EmitToString(), Is.EqualTo(expected));
		}
	}
}
