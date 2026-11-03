using ObjectPPtrAccessList = AssetRipper.Assets.Generics.PPtrAccessList<AssetRipper.Assets.Metadata.IPPtr<AssetRipper.Assets.IUnityObjectBase>, AssetRipper.Assets.IUnityObjectBase>;

namespace AssetRipper.Assets.Tests;

internal class PPtrAccessListTests
{
	[Test]
	public async Task EmptyListIsImmutable()
	{

		{
			await Assert.ThrowsAsync<NotSupportedException>(() => Task.Run(() => ObjectPPtrAccessList.Empty.Add(null)));
			await Assert.ThrowsAsync<NotSupportedException>(() => Task.Run(() => ObjectPPtrAccessList.Empty.AddNew()));
		}
	}

	[Test]
	public async Task EmptyListIsEmpty()
	{
		await Assert.That(ObjectPPtrAccessList.Empty).IsEmpty();
	}

	[Test]
	public async Task EmptyListThrowsForAccessingFirstElement()
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Run(() => ObjectPPtrAccessList.Empty[0]));
	}
}