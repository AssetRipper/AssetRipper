using AssetRipper.Assets;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Export.UnityProjects
{
	public class DummyAssetExporter : IAssetExporter
	{
		/// <summary>
		/// Setup exporting of the specified class type.
		/// </summary>
		/// <param name="classType">The class id of assets we are setting these parameters for.</param>
		/// <param name="isEmptyCollection">
		/// True: an exception will be thrown if the asset is referenced by another asset.<br/>
		/// False: any references to this asset will be replaced with a missing reference.
		/// </param>
		/// <param name="isMetaType"><see cref="AssetType.Meta"/> or <see cref="AssetType.Serialized"/>?</param>
		public void SetUpClassType(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			m_emptyTypes.Add(classType, isEmptyCollection);
			m_metaTypes.Add(classType, isMetaType);
		}

		public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (m_emptyTypes.TryGetValue((ClassIDType)asset.ClassID, out bool isEmptyCollection))
			{
				if (isEmptyCollection)
				{
					exportCollection = EmptyExportCollection.Instance;
				}
				else
				{
					exportCollection = new SkipExportCollection(this, asset);
				}
			}
			else
			{
				throw new NotSupportedException(asset.ClassID.ToString());
			}
			return true;
		}

		public AssetType ToExportType(IUnityObjectBase asset)
		{
			ToUnknownExportType((ClassIDType)asset.ClassID, out AssetType assetType);
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
			ClassIDType classID = ClassIDTypeMap.Dictionary[type];
			return ToUnknownExportType(classID, out assetType);
		}

		private readonly Dictionary<ClassIDType, bool> m_emptyTypes = new();
		private readonly Dictionary<ClassIDType, bool> m_metaTypes = new();
	}
}
