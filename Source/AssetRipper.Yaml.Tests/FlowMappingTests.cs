namespace AssetRipper.Yaml.Tests;

public class FlowMappingTests
{
	[Test]
	public async Task Vector2FlowMappingTest()
	{
		YamlMappingNode mappingNode = new(MappingStyle.Flow)
		{
			{ "x", 2 },
			{ "y", 3 }
		};
		await Assert.That(mappingNode.EmitToString()).IsEqualTo("{x: 2, y: 3}");
	}
}
