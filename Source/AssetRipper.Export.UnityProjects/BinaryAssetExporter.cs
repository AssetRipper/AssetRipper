using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.IO.Files;

namespace AssetRipper.Export.UnityProjects
{
	public class BinaryAssetExporter : IAssetExporter
	{
		public virtual bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			throw new NotSupportedException();
		}

		public virtual void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public virtual bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			throw new NotSupportedException();
		}

		public virtual void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			throw new NotSupportedException();
		}

		public virtual bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			exportCollection = new AssetExportCollection<IUnityObjectBase>(this, asset);
			return true;
		}

		public AssetType ToExportType(IUnityObjectBase asset)
		{
			return AssetType.Meta;
		}

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}

		protected static bool IsValidData(byte[]? data) => data != null && data.Length > 0;
	}
}
