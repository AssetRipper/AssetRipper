namespace AssetRipper.Yaml.Tests;

public class YamlTagTests
{
	[Test]
	public async Task EmptyTagIsNotEmitted()
	{
		YamlTag tag = new("!", string.Empty);

		await Assert.That(tag.ToString()).IsEqualTo(string.Empty);
		await Assert.That(tag.ToHeaderString()).IsEqualTo(string.Empty);
	}
}
