using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Metadata;

public readonly record struct AssetInfo(AssetCollection Collection, long PathID, int ClassID)
{
	public static AssetInfo MakeDummyAssetInfo(int classID)
	{
		return new AssetInfo(dummyBundle.Collection, -1, classID);
	}

	private static readonly DummyBundle dummyBundle = new();

	private sealed class DummyAssetCollection : AssetCollection
	{
		public DummyAssetCollection(Bundle bundle) : base(bundle)
		{
		}
	}

	private sealed class DummyBundle : Bundle
	{
		public DummyAssetCollection Collection { get; }
		public override string Name => nameof(DummyBundle);
		public DummyBundle()
		{
			Collection = new DummyAssetCollection(this);
		}
	}
}
