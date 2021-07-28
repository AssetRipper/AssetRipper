using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using System;
using System.Collections.Generic;
using UnityObject = AssetRipper.Classes.Object.UnityObject;

namespace AssetRipper.Project.Exporters
{
	internal class DummyAssetExporter : IAssetExporter
	{
		public void SetUpClassType(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			m_emptyTypes[classType] = isEmptyCollection;
			m_metaTypes[classType] = isMetaType;
		}

		public bool IsHandle(UnityObject asset, ExportOptions options)
		{
			return true;
		}

		public bool Export(IExportContainer container, UnityObject asset, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, UnityObject asset, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObject> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<UnityObject> assets, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset)
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

		public AssetType ToExportType(UnityObject asset)
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

		private readonly Dictionary<ClassIDType, bool> m_emptyTypes = new Dictionary<ClassIDType, bool>();
		private readonly Dictionary<ClassIDType, bool> m_metaTypes = new Dictionary<ClassIDType, bool>();
	}
}
