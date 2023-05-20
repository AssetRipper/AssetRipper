using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Tests;

public class TemporaryBundleTests
{
	[Test]
	public void AddNew_AddsNewTemporaryAssetCollectionToList()
	{
		// Arrange
		TemporaryBundle bundle = new();
		int previousCount = bundle.Collections.Count;

		// Act
		TemporaryAssetCollection newCollection = bundle.AddNew();

		// Assert
		Assert.IsNotNull(newCollection);
		Assert.That(bundle.Collections, Has.Count.EqualTo(previousCount + 1));
		Assert.That(bundle.Collections, Does.Contain(newCollection));
	}

	[Test]
	public void Name_ReturnsExpectedString()
	{
		// Arrange
		TemporaryBundle bundle = new();

		// Act
		string result = bundle.Name;

		// Assert
		Assert.That(result, Is.EqualTo(nameof(TemporaryBundle)));
	}
}
