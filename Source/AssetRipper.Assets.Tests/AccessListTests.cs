using AssetRipper.Assets.Generics;

namespace AssetRipper.Assets.Tests;

internal class AccessListTests
{
	[Test]
	public void EmptyListToArray()
	{
		AssetList<int> list = new();
		AccessList<int, int> accessList = new(list);
		int[] array = accessList.ToArray();
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
		AccessList<int, int> accessList = new(list);
		int[] array = accessList.ToArray();
		Assert.That(array, Has.Length.EqualTo(2));
	}
}
