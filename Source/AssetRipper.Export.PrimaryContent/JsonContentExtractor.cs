using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using System.Web;

namespace AssetRipper.Export.PrimaryContent;

public class JsonContentExtractor : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		exportCollection = new JsonExportCollection(this, asset);
		return true;
	}

	public bool Export(IUnityObjectBase asset, string filePath, FileSystem fileSystem)
	{
		using Stream stream = fileSystem.File.Create(filePath);
		using StreamWriter writer = new(stream);
		asset.WalkStandard(new JsonWalker(asset.Collection, writer));
		return true;
	}

	private sealed class JsonExportCollection : SingleExportCollection<IUnityObjectBase>
	{
		public JsonExportCollection(IContentExtractor contentExtractor, IUnityObjectBase asset) : base(contentExtractor, asset)
		{
		}

		protected override string ExportExtension => "json";
	}

	private sealed class JsonWalker(AssetCollection collection, TextWriter textWriter) : DefaultJsonWalker(textWriter)
	{
		public override void VisitPPtr<TAsset>(PPtr<TAsset> pptr)
		{
			AssetCollection? targetCollection = pptr.FileID >= 0 && pptr.FileID < collection.Dependencies.Count
				? collection.Dependencies[pptr.FileID]
				: null;

			if (targetCollection is null)
			{
				base.VisitPPtr(pptr);
			}
			else
			{
				Writer.Write("{ \"m_Collection\": \"");
				Writer.Write(HttpUtility.JavaScriptStringEncode(collection.Name));
				Writer.Write("\", \"m_PathID\": ");
				Writer.Write(pptr.PathID);
				Writer.Write(" }");
			}
		}
	}
}
