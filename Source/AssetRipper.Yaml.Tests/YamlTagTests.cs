using NUnit.Framework;

namespace AssetRipper.Yaml.Tests;

public class YamlTagTests
{
	[Test]
	public void EmptyTagIsNotEmitted()
	{
		YamlTag tag = new("!", string.Empty);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(tag.ToString(), Is.EqualTo(string.Empty));
			Assert.That(tag.ToHeaderString(), Is.EqualTo(string.Empty));
		}
	}
}
