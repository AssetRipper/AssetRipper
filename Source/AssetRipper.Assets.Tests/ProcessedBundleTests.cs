using AssetRipper.Assets.Bundles;

namespace AssetRipper.Assets.Tests;

public class ProcessedBundleTests
{
	[Test]
	public async Task ProcessedBundle_DefaultConstructor_NameShouldNotBeEmpty()
	{
		// Arrange
		ProcessedBundle bundle = new();

		// Act

		// Assert
		await Assert.That(bundle.Name).IsNotEmpty();
	}

	[Test]
	public async Task ProcessedBundle_ArgumentConstructor_Null_ShouldNotThrowException()
	{
		// Arrange
		string? name = null;

		// Act

		// Assert
		await Assert.That(() => new ProcessedBundle(name)).ThrowsNothing();
	}

	[Test]
	public async Task ProcessedBundle_ArgumentConstructor_EmptyString_ShouldNotThrowException()
	{
		// Arrange
		string name = string.Empty;

		// Act

		// Assert
		await Assert.That(() => new ProcessedBundle(name)).ThrowsNothing();
	}

	[Test]
	public async Task ProcessedBundle_ArgumentConstructor_ValidName_ShouldNotBeNull()
	{
		// Arrange
		string name = "TestBundleName";

		// Act
		ProcessedBundle bundle = new ProcessedBundle(name);

		// Assert
		await Assert.That(bundle).IsNotNull();
		await Assert.That(bundle.Name).IsEqualTo(name);
	}
}