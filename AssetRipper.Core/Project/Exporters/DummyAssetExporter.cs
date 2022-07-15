using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Exporters
{
	public class DummyAssetExporter : IAssetExporter
	{
		public void SetUpClassType(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			m_emptyTypes[classType] = isEmptyCollection;
			m_metaTypes[classType] = isMetaType;
		}

		public bool IsHandle(IUnityObjectBase asset)
		{
			return true;
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			if (m_emptyTypes.TryGetValue(asset.ClassID, out bool isEmptyCollection))
			{
				if (isEmptyCollection)
				{
					return new EmptyExportCollection();
				}
				else
				{
					return new SkipExportCollection(this, asset);
				}
			}
			else
			{
				throw new NotSupportedException(asset.ClassID.ToString());
			}
		}

		public AssetType ToExportType(IUnityObjectBase asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			if (m_metaTypes.TryGetValue(classID, out bool isMetaType))
			{
				assetType = isMetaType ? AssetType.Meta : AssetType.Serialized;
				return true;
			}
			else
			{
				throw new NotSupportedException(classID.ToString());
			}
		}

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			ClassIDType classID = VersionHandling.VersionManager.AssetFactory.GetClassIdForType(type);
			return ToUnknownExportType(classID, out assetType);
		}

		private readonly Dictionary<ClassIDType, bool> m_emptyTypes = new Dictionary<ClassIDType, bool>();
		private readonly Dictionary<ClassIDType, bool> m_metaTypes = new Dictionary<ClassIDType, bool>();
	}
}
