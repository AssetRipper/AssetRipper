using AssetRipper.Assets.Generics;

namespace AssetRipper.Assets.Tests;

internal class AccessListTests
{
	[Test]
	public async Task EmptyListToArray()
	{
		AssetList<int> list = new();
		AccessList<int, int> accessList = new(list);
		int[] array = accessList.ToArray();
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
		AccessList<int, int> accessList = new(list);
		int[] array = accessList.ToArray();
		await Assert.That(array.Length).IsEqualTo(2);
	}
}