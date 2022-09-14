using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Structure;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.VersionHandling;
using AssetRipper.IO.Endian;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Files.SerializedFiles
{
	/// <summary>
	/// A serialized file without any actual file backing it
	/// </summary>
	public class VirtualSerializedFile : ISerializedFile
	{
		public VirtualSerializedFile(LayoutInfo layout)
		{
			Layout = layout;
		}

		public IUnityObjectBase GetAsset(long pathID)
		{
			IUnityObjectBase? asset = TryGetAsset(pathID);
			if (asset is null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}
			return asset;
		}

		public IUnityObjectBase GetAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualFileIndex)
			{
				return GetAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public IUnityObjectBase? TryGetAsset(long pathID)
		{
			m_assets.TryGetValue(pathID, out IUnityObjectBase? asset);
			return asset;
		}

		public IUnityObjectBase? TryGetAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualFileIndex)
			{
				return TryGetAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public ObjectInfo GetAssetEntry(long pathID)
		{
			throw new NotSupportedException();
		}

		public PPtr<T> CreatePPtr<T>(T asset) where T : IUnityObjectBase
		{
			if (asset.SerializedFile == this)
			{
				return new PPtr<T>(VirtualFileIndex, asset.PathID);
			}
			throw new Exception($"Asset '{asset}' doesn't belong to {nameof(VirtualSerializedFile)}");
		}

		public IEnumerable<IUnityObjectBase> FetchAssets()
		{
			return m_assets.Values;
		}

		public T CreateAsset<T>(Func<AssetInfo, T> factory) where T : IUnityObjectBase
		{
			ClassIDType classID = VersionManager.AssetFactory.GetClassIdForType(typeof(T));
			return CreateAsset<T>(classID, factory);
		}

		public T CreateAsset<T>(ClassIDType classID, Func<AssetInfo, T> factory) where T : IUnityObjectBase
		{
			AssetInfo assetInfo = CreateAssetInfo(classID);
			T instance = factory(assetInfo);
			m_assets.Add(instance.PathID, instance);
			return instance;
		}

		public T CreateAsset<T>(ClassIDType classID) where T : IUnityObjectBase
		{
			return CreateAsset<T>(classID, Version);
		}

		public T CreateAsset<T>(UnityVersion version) where T : IUnityObjectBase
		{
			ClassIDType classID = VersionManager.AssetFactory.GetClassIdForType(typeof(T));
			return CreateAsset<T>(classID, version);
		}

		public T CreateAsset<T>(ClassIDType classID, UnityVersion version) where T : IUnityObjectBase
		{
			AssetInfo assetInfo = CreateAssetInfo(classID);
			IUnityObjectBase? asset = VersionManager.AssetFactory.CreateAsset(assetInfo, version);
			if (asset is null)
			{
				throw new ArgumentException($"Could not create asset with id: {classID}", nameof(classID));
			}
			if (asset is T instance)
			{
				m_assets.Add(instance.PathID, instance);
				return instance;
			}
			else
			{
				throw new ArgumentException($"Asset type {asset.GetType()} is not assignable to {typeof(T)}", nameof(classID));
			}
		}

		public void AddAsset(IUnityObjectBase asset, ClassIDType classID)
		{
			asset.AssetInfo = CreateAssetInfo(classID);
			m_assets.Add(asset.PathID, asset);
		}

		private AssetInfo CreateAssetInfo(ClassIDType classID)
		{
			return new AssetInfo(this, ++m_nextId, classID);
		}

		public string Name => nameof(VirtualSerializedFile);
		public BuildTarget Platform => Layout.Platform;
		public UnityVersion Version => Layout.Version;
		public TransferInstructionFlags Flags => Layout.Flags;
		public EndianType EndianType => EndianType.LittleEndian;

		public bool IsScene => throw new NotSupportedException();

		public LayoutInfo Layout { get; }
		public IFileCollection Collection => throw new NotSupportedException();
		public IAssemblyManager AssemblyManager => throw new NotSupportedException();
		public IReadOnlyList<FileIdentifier> Dependencies => throw new NotSupportedException();

		public const int VirtualFileIndex = -1;

		private readonly Dictionary<long, IUnityObjectBase> m_assets = new Dictionary<long, IUnityObjectBase>();

		private long m_nextId;
	}
}
