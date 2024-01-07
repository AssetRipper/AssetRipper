using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Import.AssetCreation;
using AssetRipper.IO.Files;

namespace AssetRipper.Export.UnityProjects.RawAssets
{
	public sealed class UnknownObjectExporter : IAssetExporter
	{
		public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is UnknownObject @object)
			{
				exportCollection = new UnknownExportCollection(this, @object);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			File.WriteAllBytes(path, ((UnknownObject)asset).RawData);
			return true;
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			if (Export(container, asset, path))
			{
				callback?.Invoke(container, asset, path);
			}
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			bool success = true;
			foreach (IUnityObjectBase asset in assets)
			{
				success &= Export(container, asset, path);
			}
			return success;
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			foreach (IUnityObjectBase asset in assets)
			{
				Export(container, asset, path, callback);
			}
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
	}
}
