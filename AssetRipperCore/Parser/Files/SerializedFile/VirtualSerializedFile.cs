using AssetRipper.Layout;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes;
using AssetRipper.Classes.Misc;
using AssetRipper.Classes.Utils.Extensions;
using AssetRipper.Parser.Files.SerializedFiles.Parser;
using AssetRipper.IO.Asset;
using AssetRipper.Structure;
using AssetRipper.Structure.Assembly.Managers;
using System;
using System.Collections.Generic;
using UnityObject = AssetRipper.Classes.Object.UnityObject;

namespace AssetRipper.Parser.Files.SerializedFiles
{
	public class VirtualSerializedFile : ISerializedFile
	{
		public VirtualSerializedFile(AssetLayout layout)
		{
			Layout = layout;
		}

		public UnityObject GetAsset(long pathID)
		{
			UnityObject asset = FindAsset(pathID);
			if (asset == null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}
			return asset;
		}

		public UnityObject GetAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualFileIndex)
			{
				return GetAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public UnityObject FindAsset(long pathID)
		{
			m_assets.TryGetValue(pathID, out UnityObject asset);
			return asset;
		}

		public UnityObject FindAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualFileIndex)
			{
				return FindAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public UnityObject FindAsset(ClassIDType classID)
		{
			foreach (UnityObject asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					return asset;
				}
			}
			return null;
		}

		public UnityObject FindAsset(ClassIDType classID, string name)
		{
			foreach (UnityObject asset in FetchAssets())
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

		public PPtr<T> CreatePPtr<T>(T asset) where T : UnityObject
		{
			if (asset.File == this)
			{
				return new PPtr<T>(VirtualFileIndex, asset.PathID);
			}
			throw new Exception($"Asset '{asset}' doesn't belong to {nameof(VirtualSerializedFile)}");
		}

		public IEnumerable<UnityObject> FetchAssets()
		{
			return m_assets.Values;
		}

		public T CreateAsset<T>(Func<AssetInfo, T> instantiator) where T : UnityObject
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			AssetInfo assetInfo = new AssetInfo(this, ++m_nextId, classID);
			T instance = instantiator(assetInfo);
			m_assets.Add(instance.PathID, instance);
			return instance;
		}

		public string Name => nameof(VirtualSerializedFile);
		public Platform Platform => Layout.Info.Platform;
		public Version Version => Layout.Info.Version;
		public TransferInstructionFlags Flags => Layout.Info.Flags;

		public bool IsScene => throw new NotSupportedException();

		public AssetLayout Layout { get; }
		public IFileCollection Collection => throw new NotSupportedException();
		public IAssemblyManager AssemblyManager => throw new NotSupportedException();
		public IReadOnlyList<FileIdentifier> Dependencies => throw new NotSupportedException();

		public const int VirtualFileIndex = -1;

		private readonly Dictionary<long, UnityObject> m_assets = new Dictionary<long, UnityObject>();

		private long m_nextId;
	}
}
