using NUnit.Framework;

namespace AssetRipper.Yaml.Tests;

public class YamlScalarNodeTests
{
	[Test]
	public void NullCharacterIsDoubleQuotedAndEscaped()
	{
		YamlScalarNode node = new("\0");
		Assert.Multiple(() =>
		{
			Assert.That(node.Value, Is.EqualTo("\0"));
			Assert.That(node.NodeType, Is.EqualTo(YamlNodeType.Scalar));
			Assert.That(node.Style, Is.EqualTo(ScalarStyle.DoubleQuoted));
			Assert.That(ToString(node), Is.EqualTo("\"\\u0000\""));
		});
	}

	[Test]
	public void EndOfTextCharacterCausesDoubleQuoting()
	{
		const string someText = "Some text\u0003";
		YamlScalarNode node = new(someText);
		Assert.Multiple(() =>
		{
			Assert.That(node.Value, Is.EqualTo(someText));
			Assert.That(node.NodeType, Is.EqualTo(YamlNodeType.Scalar));
			Assert.That(node.Style, Is.EqualTo(ScalarStyle.DoubleQuoted));
			Assert.That(ToString(node), Is.EqualTo("\"Some text\\u0003\""));
		});
	}

	[Test]
	public void AsciiCharactersUsePlainStyle()
	{
		const string asciiCharacters = "Ascii Characters";
		YamlScalarNode node = new(asciiCharacters);
		Assert.Multiple(() =>
		{
			Assert.That(node.Value, Is.EqualTo(asciiCharacters));
			Assert.That(node.NodeType, Is.EqualTo(YamlNodeType.Scalar));
			Assert.That(node.Style, Is.EqualTo(ScalarStyle.Plain));
			Assert.That(ToString(node), Is.EqualTo(asciiCharacters));
		});
	}

	private static string ToString(YamlNode node)
	{
		StringWriter writer = new();
		Emitter emitter = new(writer, false);
		node.Emit(emitter);
		return writer.ToString();
	}
}
