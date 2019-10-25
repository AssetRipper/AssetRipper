using System;
using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.SerializedFiles
{
	public class VirtualSerializedFile : ISerializedFile
	{
		public VirtualSerializedFile(ExportOptions options)
		{
			Version = options.Version;
			Platform = options.Platform;
			Flags = options.Flags;
		}

		public Object GetAsset(long pathID)
		{
			Object asset = FindAsset(pathID);
			if (asset == null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}
			return asset;
		}

		public Object GetAsset(int fileIndex, long pathID)
		{
			if (fileIndex == VirtualFileIndex)
			{
				return GetAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public Object FindAsset(long pathID)
		{
			m_assets.TryGetValue(pathID, out Object asset);
			return asset;
		}

		public Object FindAsset(int fileIndex, long pathID)
		{
			if(fileIndex == VirtualFileIndex)
			{
				return FindAsset(pathID);
			}
			throw new NotSupportedException();
		}

		public Object FindAsset(ClassIDType classID)
		{
			foreach(Object asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					return asset;
				}
			}
			return null;
		}

		public Object FindAsset(ClassIDType classID, string name)
		{
			foreach (Object asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					NamedObject namedAsset = (NamedObject)asset;
					if(namedAsset.ValidName == name)
					{
						return namedAsset;
					}
					return asset;
				}
			}
			return null;
		}

		public AssetEntry GetAssetEntry(long pathID)
		{
			throw new NotSupportedException();
		}

		public ClassIDType GetClassID(long pathID)
		{
			throw new NotSupportedException();
		}

		public PPtr<T> CreatePPtr<T>(T asset)
			where T : Object
		{
			if(asset.File == this)
			{
				return new PPtr<T>(VirtualFileIndex, asset.PathID);
			}
			throw new Exception($"Asset '{asset}' doesn't belong to {nameof(VirtualSerializedFile)}");
		}

		public IEnumerable<Object> FetchAssets()
		{
			return m_assets.Values;
		}

		public T CreateAsset<T>(Func<AssetInfo, T> instantiator)
			where T: Object
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			AssetInfo assetInfo = new AssetInfo(this, ++m_nextId, classID);
			T instance = instantiator(assetInfo);
			m_assets.Add(instance.PathID, instance);
			return instance;
		}

		public T CreateAsset<T>(EngineGUID guid, Func<AssetInfo, T> instantiator)
			where T : Object
		{
			ClassIDType classID = typeof(T).ToClassIDType();
			AssetInfo assetInfo = new AssetInfo(this, ++m_nextId, classID, guid);
			T instance = instantiator(assetInfo);
			m_assets.Add(instance.PathID, instance);
			return instance;
		}

		public string Name => nameof(VirtualSerializedFile);
		public Platform Platform { get; }
		public Version Version { get; }
		public TransferInstructionFlags Flags { get; }

		public bool IsScene => throw new NotSupportedException();

		public IFileCollection Collection => throw new NotSupportedException();
		public IAssemblyManager AssemblyManager => throw new NotSupportedException();
		public IReadOnlyList<FileIdentifier> Dependencies => throw new NotSupportedException();
		
		public const int VirtualFileIndex = -1;

		private readonly Dictionary<long, Object> m_assets = new Dictionary<long, Object>();

		private long m_nextId;
	}
}
