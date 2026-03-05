using AssetRipper.Processing.PrefabOutlining;

namespace AssetRipper.Tests.PrefabOutlining;

public class GameObjectNameCleanerTests
{
	[TestCase("", "GameObject")]
	[TestCase("   ", "GameObject")]
	[TestCase("(Clone)", "GameObject")]
	[TestCase("Enemy(Clone)", "Enemy")]
	[TestCase("Enemy (Clone)", "Enemy")]
	[TestCase("Enemy (1)", "Enemy")]
	[TestCase("Enemy(2)", "Enemy")]
	[TestCase("Enemy (Clone) (12)", "Enemy")]
	[TestCase("Enemy (12) (Clone)", "Enemy")]
	[TestCase("Enemy Boss", "Enemy Boss")]
	public void CleanName_ReturnsExpectedValue(string originalName, string expectedName)
	{
		string cleaned = GameObjectNameCleaner.CleanName(originalName);
		Assert.That(cleaned, Is.EqualTo(expectedName));
	}
}
