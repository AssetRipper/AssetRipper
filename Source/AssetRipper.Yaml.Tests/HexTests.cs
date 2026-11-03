namespace AssetRipper.Yaml.Tests;

internal class HexTests
{
	[Test]
	public async Task OneFloatTest()
	{
		YamlScalarNode node = YamlScalarNode.CreateHex(1f);
		await Assert.That(node.ToString()).IsEqualTo("0x3f800000(1)");
	}
}
