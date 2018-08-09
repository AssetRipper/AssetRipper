using System;
using System.Collections.Generic;
using UtinyRipper.Classes;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.SerializedFiles
{
	public class VirtualSerializedFile : ISerializedFile
	{
		public Object GetObject(long pathID)
		{
			Object @object = FindObject(pathID);
			if (@object == null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}

			return @object;
		}

		public Object GetObject(int fileIndex, long pathID)
		{
			if (fileIndex == PPtr<Object>.VirtualFileIndex)
			{
				return GetObject(pathID);
			}
			throw new NotSupportedException();
		}

		public Object FindObject(long pathID)
		{
			m_assets.TryGetValue(pathID, out Object asset);
			return asset;
		}

		public Object FindObject(int fileIndex, long pathID)
		{
			if(fileIndex == PPtr<Object>.VirtualFileIndex)
			{
				return FindObject(pathID);
			}
			throw new NotSupportedException();
		}

		public AssetEntry GetAssetEntry(long pathID)
		{
			throw new NotSupportedException();
		}

		public ClassIDType GetClassID(long pathID)
		{
			throw new NotSupportedException();
		}

		public IEnumerable<Object> FetchAssets()
		{
			return m_assets.Values;
		}
		
		public AssetInfo CreateAssetInfo(ClassIDType classID)
		{
			return new AssetInfo(this, ++m_nextId, classID);
		}

		public void AddAsset(Object asset)
		{
			m_assets.Add(asset.PathID, asset);
		}

		public string Name => throw new NotSupportedException();
		public Platform Platform => throw new NotSupportedException();
		public Version Version => throw new NotSupportedException();
		public TransferInstructionFlags Flags => throw new NotSupportedException();

		public bool IsScene => throw new NotSupportedException();

		public IFileCollection Collection => throw new NotSupportedException();
		public IReadOnlyList<FileIdentifier> Dependencies => throw new NotSupportedException();

		private readonly Dictionary<long, Object> m_assets = new Dictionary<long, Object>();

		private long m_nextId;
	}
}
