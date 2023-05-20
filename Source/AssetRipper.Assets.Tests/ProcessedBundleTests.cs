using AssetRipper.Assets.Bundles;

namespace AssetRipper.Assets.Tests;

public class ProcessedBundleTests
{
	[Test]
	public void ProcessedBundle_DefaultConstructor_NameShouldNotBeEmpty()
	{
		// Arrange
		ProcessedBundle bundle = new();

		// Act

		// Assert
		Assert.That(bundle.Name, Is.Not.Empty);
	}

	[Test]
	public void ProcessedBundle_ArgumentConstructor_Null_ShouldThrowException()
	{
		// Arrange
		string name = null!;

		// Act

		// Assert
		Assert.Throws<ArgumentNullException>(() => new ProcessedBundle(name));
	}

	[Test]
	public void ProcessedBundle_ArgumentConstructor_EmptyString_ShouldThrowException()
	{
		// Arrange
		string name = string.Empty;

		// Act

		// Assert
		Assert.Throws<ArgumentException>(() => new ProcessedBundle(name));
	}

	[Test]
	public void ProcessedBundle_ArgumentConstructor_ValidName_ShouldNotBeNull()
	{
		// Arrange
		string name = "TestBundleName";

		// Act
		ProcessedBundle bundle = new ProcessedBundle(name);

		// Assert
		Assert.That(bundle, Is.Not.Null);
		Assert.That(bundle.Name, Is.EqualTo(name));
	}
}
