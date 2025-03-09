using NUnit.Framework;

namespace AssetRipper.Yaml.Tests;

internal class HexTests
{
	[Test]
	public void OneFloatTest()
	{
		YamlScalarNode node = YamlScalarNode.CreateHex(1f);
		Assert.That(node.ToString(), Is.EqualTo("0x3f800000(1)"));
	}
}
