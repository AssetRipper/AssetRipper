using ObjectPPtrAccessList = AssetRipper.Assets.Generics.PPtrAccessList<AssetRipper.Assets.Metadata.IPPtr<AssetRipper.Assets.IUnityObjectBase>, AssetRipper.Assets.IUnityObjectBase>;

namespace AssetRipper.Assets.Tests;

internal class PPtrAccessListTests
{
	[Test]
	public void EmptyListIsImmutable()
	{
		using (Assert.EnterMultipleScope())
		{
			Assert.Throws<NotSupportedException>(() =>
			{
				ObjectPPtrAccessList.Empty.Add(null);
			});
			Assert.Throws<NotSupportedException>(() =>
			{
				ObjectPPtrAccessList.Empty.AddNew();
			});
		}
	}

	[Test]
	public void EmptyListIsEmpty()
	{
		Assert.That(ObjectPPtrAccessList.Empty, Is.Empty);
	}

	[Test]
	public void EmptyListThrowsForAccessingFirstElement()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
		{
			_ = ObjectPPtrAccessList.Empty[0];
		});
	}
}
