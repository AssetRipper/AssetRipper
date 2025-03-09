using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public class MultipleExportCollection<T> : SingleExportCollection<T> where T : IUnityObjectBase
{
	public MultipleExportCollection(IContentExtractor contentExtractor, T asset) : base(contentExtractor, asset)
	{
	}

	protected override bool ExportInner(string filePath, string dirPath, FileSystem fileSystem)
	{
		return ContentExtractor.Export(Assets, filePath, fileSystem);
	}

	public void AddAsset(IUnityObjectBase asset)
	{
		if (!ReferenceEquals(asset, Asset))
		{
			secondaryAssets.Add(asset);
		}
	}

	public void AddAssets(IEnumerable<IUnityObjectBase> assets)
	{
		foreach (IUnityObjectBase asset in assets)
		{
			AddAsset(asset);
		}
	}

	public override bool Contains(IUnityObjectBase asset)
	{
		return base.Contains(asset) || secondaryAssets.Contains(asset);
	}

	private readonly HashSet<IUnityObjectBase> secondaryAssets = new();

	public override IEnumerable<IUnityObjectBase> Assets
	{
		get
		{
			yield return Asset;
			foreach (IUnityObjectBase asset in secondaryAssets)
			{
				yield return asset;
			}
		}
	}
}
