using AssetRipper.Assets.Generics;

namespace AssetRipper.Assets.Tests;

internal class AssetListTests
{
	[Test]
	public void EmptyListToArray()
	{
		AssetList<int> list = new();
		int[] array = list.ToArray();
		Assert.That(array, Is.Empty);
	}

	[Test]
	public void NonemptyListToArray()
	{
		AssetList<int> list = new()
		{
			1,
			2
		};
		int[] array = list.ToArray();
		Assert.That(array, Has.Length.EqualTo(2));
	}
}
