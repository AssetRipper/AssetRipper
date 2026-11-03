using AssetRipper.Assets.Generics;

namespace AssetRipper.Assets.Tests;

internal class AssetListTests
{
	[Test]
	public async Task EmptyListToArray()
	{
		AssetList<int> list = new();
		int[] array = list.ToArray();
		await Assert.That(array).IsEmpty();
	}

	[Test]
	public async Task NonemptyListToArray()
	{
		AssetList<int> list = new()
		{
			1,
			2
		};
		int[] array = list.ToArray();
		await Assert.That(array.Length).IsEqualTo(2);
	}
}