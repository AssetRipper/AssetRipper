using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Structure;
using AssetRipper.Core.Structure.Assembly.Managers;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Parser.Files.SerializedFiles
{
	/// <summary>
	/// A serialized file without any actual file backing it
	/// </summary>
	public class VirtualSerializedFile : ISerializedFile
	{
		public VirtualSerializedFile(AssetLayout layout)
		{
			Layout = layout;
		}

		public UnityObjectBase GetAsset(long pathID)
		{
			UnityObjectBase asset = FindAsset(pathID);
			if (asset == null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}
			return asset;
		}

		public UnityObjectBase GetAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualFileIndex)
			{
				return GetAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public UnityObjectBase FindAsset(long pathID)
		{
			m_assets.TryGetValue(pathID, out UnityObjectBase asset);
			return asset;
		}

		public UnityObjectBase FindAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualFileIndex)
			{
				return FindAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public UnityObjectBase FindAsset(ClassIDType classID)
		{
			foreach (UnityObjectBase asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					return asset;
				}
			}
			return null;
		}

		public UnityObjectBase FindAsset(ClassIDType classID, string name)
		{
			foreach (UnityObjectBase asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					NamedObject namedAsset = (NamedObject)asset;
					if (namedAsset.ValidName == name)
					{
						return namedAsset;
					}
					return asset;
				}
			}
			return null;
		}

		public ObjectInfo GetAssetEntry(long pathID)
		{
			throw new NotSupportedException();
		}

		public ClassIDType GetAssetType(long pathID)
		{
			return m_assets[pathID].ClassID;
		}

		public PPtr<T> CreatePPtr<T>(T obj) where T : UnityObjectBase
		{
			if (obj is not Object asset)
				throw new NotSupportedException();

			if (asset.File == this)
			{
				return new PPtr<T>(VirtualFileIndex, asset.PathID);
			}
			throw new Exception($"Asset '{asset}' doesn't belong to {nameof(VirtualSerializedFile)}");
		}

		public IEnumerable<UnityObjectBase> FetchAssets()
		{
			return m_assets.Values;
		}

		public T CreateAsset<T>(Func<AssetInfo, T> instantiator) where T : UnityObjectBase
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			AssetInfo assetInfo = new AssetInfo(this, ++m_nextId, classID);
			T instance = instantiator(assetInfo);
			m_assets.Add(instance.PathID, instance);
			return instance;
		}

		public string Name => nameof(VirtualSerializedFile);
		public Platform Platform => Layout.Info.Platform;
		public UnityVersion Version => Layout.Info.Version;
		public TransferInstructionFlags Flags => Layout.Info.Flags;

		public bool IsScene => throw new NotSupportedException();

		public AssetLayout Layout { get; }
		public IFileCollection Collection => throw new NotSupportedException();
		public IAssemblyManager AssemblyManager => throw new NotSupportedException();
		public IReadOnlyList<FileIdentifier> Dependencies => throw new NotSupportedException();

		public const int VirtualFileIndex = -1;

		private readonly Dictionary<long, UnityObjectBase> m_assets = new Dictionary<long, UnityObjectBase>();

		private long m_nextId;
	}
}
