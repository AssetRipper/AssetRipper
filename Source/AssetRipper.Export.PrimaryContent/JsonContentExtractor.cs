using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public class JsonContentExtractor : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		exportCollection = new JsonExportCollection(this, asset);
		return true;
	}

	public bool Export(IUnityObjectBase asset, string filePath)
	{
		string json = new DefaultJsonWalker().SerializeStandard(asset);
		File.WriteAllText(filePath, json);
		return true;
	}

	private sealed class JsonExportCollection : SingleExportCollection<IUnityObjectBase>
	{
		public JsonExportCollection(IContentExtractor contentExtractor, IUnityObjectBase asset) : base(contentExtractor, asset)
		{
		}

		protected override string ExportExtension => "json";
	}
}
