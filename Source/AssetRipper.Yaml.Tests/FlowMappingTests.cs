using NUnit.Framework;

namespace AssetRipper.Yaml.Tests
{
	public static class FlowMappingTests
	{
		[Test]
		public static void Vector2FlowMappingTest()
		{
			StringWriter writer = new();
			Emitter emitter = new Emitter(writer, false);
			YamlMappingNode mappingNode = new YamlMappingNode(MappingStyle.Flow);
			mappingNode.Add("x", 2);
			mappingNode.Add("y", 3);
			mappingNode.Emit(emitter);
			string output = writer.ToString();
			Assert.That(output, Is.EqualTo("{x: 2, y: 3}"));
		}
	}
}
