using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets.Cloning
{
	public readonly record struct PPtrConverter(AssetCollection SourceCollection, AssetCollection TargetCollection, IAssetResolver Resolver)
	{
		public PPtrConverter(AssetCollection SourceCollection, AssetCollection TargetCollection)
			: this(SourceCollection, TargetCollection, DefaultAssetResolver.Shared)
		{
		}

		public PPtrConverter(IUnityObjectBase sourceAsset, IUnityObjectBase targetAsset) : this(sourceAsset.Collection, targetAsset.Collection)
		{
		}

		public PPtr<T> Convert<T>(PPtr sourcePPtr) where T : IUnityObjectBase
		{
			IUnityObjectBase? asset = SourceCollection.TryGetAsset(sourcePPtr);
			return TargetCollection.ForceCreatePPtr(Resolver.Resolve<T>(asset));
		}
	}
}
