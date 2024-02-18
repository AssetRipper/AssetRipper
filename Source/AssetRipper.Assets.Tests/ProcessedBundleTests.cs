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
	public void ProcessedBundle_ArgumentConstructor_Null_ShouldNotThrowException()
	{
		// Arrange
		string? name = null;

		// Act

		// Assert
		Assert.DoesNotThrow(() => new ProcessedBundle(name));
	}

	[Test]
	public void ProcessedBundle_ArgumentConstructor_EmptyString_ShouldNotThrowException()
	{
		// Arrange
		string name = string.Empty;

		// Act

		// Assert
		Assert.DoesNotThrow(() => new ProcessedBundle(name));
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
