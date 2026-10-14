using AssetRipper.Export.UnityProjects.Textures;

namespace AssetRipper.Tests;

internal class FlipbookLayoutTests
{
	[Test]
	public void LayersThatFitInOneColumnUseOneColumn()
	{
		Assert.That(FlipbookLayout.TryCreate(32, 32, 20, out FlipbookLayout layout), Is.True);
		Assert.That(layout, Is.EqualTo(new FlipbookLayout(1, 20)));
	}

	[TestCase(256, 256, 2048)]
	[TestCase(512, 512, 600)]
	[TestCase(2048, 2048, 64)]
	[TestCase(128, 32, 1000)]
	public void TallArraysUseAGridThatUnityCanImport(int width, int height, int depth)
	{
		Assert.That(FlipbookLayout.TryCreate(width, height, depth, out FlipbookLayout layout), Is.True);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(layout.LayerCount, Is.GreaterThanOrEqualTo(depth));
			Assert.That(width * layout.Columns, Is.LessThanOrEqualTo(FlipbookLayout.MaxImageSize));
			Assert.That(height * layout.Rows, Is.LessThanOrEqualTo(FlipbookLayout.MaxImageSize));
		}
	}

	[Test]
	public void AnExactGridIsPreferredSoNoEmptyLayersAreAdded()
	{
		Assert.That(FlipbookLayout.TryCreate(256, 256, 2048, out FlipbookLayout layout), Is.True);
		Assert.That(layout.LayerCount, Is.EqualTo(2048));
	}

	[Test]
	public void ArraysTooLargeForOneImageAreRejected()
	{
		// 2048x2048 layers fit 8 across and 8 down, so 65 can't fit.
		Assert.That(FlipbookLayout.TryCreate(2048, 2048, 65, out _), Is.False);
	}
}
