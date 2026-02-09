using NUnit.Framework;

namespace AssetRipper.Yaml.Tests;

public static class FlowMappingTests
{
	[Test]
	public static void Vector2FlowMappingTest()
	{
		YamlMappingNode mappingNode = new(MappingStyle.Flow)
		{
			{ "x", 2 },
			{ "y", 3 }
		};
		Assert.That(mappingNode.EmitToString(), Is.EqualTo("{x: 2, y: 3}"));
	}
}
