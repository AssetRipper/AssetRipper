using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes;
using uTinyRipper.Layout;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile : ISerializedFile
	{
		internal SerializedFile(GameCollection collection, SerializedFileScheme scheme)
		{
			Collection = collection ?? throw new ArgumentNullException(nameof(collection));
			FilePath = scheme.FilePath;
			NameOrigin = scheme.Name;
			Name = FilenameUtils.FixFileIdentifier(scheme.Name);
			Layout = GetLayout(collection, scheme, Name);

			Header = scheme.Header;
			Metadata = scheme.Metadata;

			for (int i = 0; i < Metadata.Object.Length; i++)
			{
				m_assetEntryLookup.Add(Metadata.Object[i].FileID, i);
			}
		}

		public static bool IsSerializedFile(string filePath)
		{
			using (Stream stream = MultiFileStream.OpenRead(filePath))
			{
				return IsSerializedFile(stream);
			}
		}

		public static bool IsSerializedFile(byte[] buffer, int offset, int size)
		{
			using (MemoryStream stream = new MemoryStream(buffer, offset, size, false))
			{
				return IsSerializedFile(stream);
			}
		}

		public static bool IsSerializedFile(Stream stream)
		{
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				return SerializedFileHeader.IsSerializedFileHeader(reader, (uint)stream.Length);
			}
		}

		public static SerializedFileScheme LoadScheme(string filePath, string fileName)
		{
			using (SmartStream fileStream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(fileStream, filePath, fileName);
			}
		}

		public static SerializedFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			return SerializedFileScheme.ReadSceme(buffer, filePath, fileName);
		}

		public static SerializedFileScheme ReadScheme(SmartStream stream, string filePath, string fileName)
		{
			return SerializedFileScheme.ReadSceme(stream, filePath, fileName);
		}

		private static AssetLayout GetLayout(GameCollection collection, SerializedFileScheme scheme, string name)
		{
			if (!SerializedFileMetadata.HasPlatform(scheme.Header.Version))
			{
				return collection.Layout;
			}
			if (FilenameUtils.IsDefaultResource(name))
			{
				return collection.Layout;
			}

			LayoutInfo info = new LayoutInfo(scheme.Metadata.UnityVersion, scheme.Metadata.TargetPlatform, scheme.Flags);
			return collection.GetLayout(info);
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
			return FindAsset(fileIndex, pathID, false);
		}

		public Object FindAsset(long pathID)
		{
			m_assets.TryGetValue(pathID, out Object asset);
			return asset;
		}

		public Object FindAsset(int fileIndex, long pathID)
		{
			return FindAsset(fileIndex, pathID, true);
		}

		public Object FindAsset(ClassIDType classID)
		{
			foreach (Object asset in FetchAssets())
			{
				if (asset.ClassID == classID)
				{
					return asset;
				}
			}

			foreach (FileIdentifier identifier in Metadata.Externals)
			{
				ISerializedFile file = Collection.FindSerializedFile(identifier.GetFilePath());
				if (file == null)
				{
					continue;
				}
				foreach (Object asset in file.FetchAssets())
				{
					if (asset.ClassID == classID)
					{
						return asset;
					}
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
					if (namedAsset.ValidName == name)
					{
						return asset;
					}
				}
			}

			foreach (FileIdentifier identifier in Metadata.Externals)
			{
				ISerializedFile file = Collection.FindSerializedFile(identifier.GetFilePath());
				if (file == null)
				{
					continue;
				}
				foreach (Object asset in file.FetchAssets())
				{
					if (asset.ClassID == classID)
					{
						NamedObject namedAsset = (NamedObject)asset;
						if (namedAsset.Name == name)
						{
							return asset;
						}
					}
				}
			}
			return null;
		}

		public ObjectInfo GetAssetEntry(long pathID)
		{
			return Metadata.Object[m_assetEntryLookup[pathID]];
		}

		public ClassIDType GetAssetType(long pathID)
		{
			return Metadata.Object[m_assetEntryLookup[pathID]].ClassID;
		}

		public PPtr<T> CreatePPtr<T>(T asset)
			where T : Object
		{
			if (asset.File == this)
			{
				return new PPtr<T>(0, asset.PathID);
			}

			for (int i = 0; i < Metadata.Externals.Length; i++)
			{
				FileIdentifier identifier = Metadata.Externals[i];
				ISerializedFile file = Collection.FindSerializedFile(identifier.GetFilePath());
				if (asset.File == file)
				{
					return new PPtr<T>(i + 1, asset.PathID);
				}
			}

			throw new Exception("Asset doesn't belong to this serialized file or its dependencies");
		}

		public IEnumerable<Object> FetchAssets()
		{
			return m_assets.Values;
		}

		public override string ToString()
		{
			return Name;
		}

		internal void ReadData(Stream stream)
		{
			using (AssetReader assetReader = new AssetReader(stream, GetEndianType(), Layout))
			{
				if (SerializedFileMetadata.HasScriptTypes(Header.Version))
				{
					foreach (LocalSerializedObjectIdentifier ptr in Metadata.ScriptTypes)
					{
						if (ptr.LocalSerializedFileIndex == 0)
						{
							int index = m_assetEntryLookup[ptr.LocalIdentifierInFile];
							ReadAsset(assetReader, ref Metadata.Object[index]);
						}
					}
				}

				for (int i = 0; i < Metadata.Object.Length; i++)
				{
					if (Metadata.Object[i].ClassID == ClassIDType.MonoScript)
					{
						if (!m_assets.ContainsKey(Metadata.Object[i].FileID))
						{
							ReadAsset(assetReader, ref Metadata.Object[i]);
						}
					}
				}

				for (int i = 0; i < Metadata.Object.Length; i++)
				{
					if (!m_assets.ContainsKey(Metadata.Object[i].FileID))
					{
						ReadAsset(assetReader, ref Metadata.Object[i]);
					}
				}
			}
		}

		private Object FindAsset(int fileIndex, long pathID, bool isSafe)
		{
			ISerializedFile file;
			if (fileIndex == 0)
			{
				file = this;
			}
			else
			{
				fileIndex--;
				if (fileIndex >= Metadata.Externals.Length)
				{
					throw new Exception($"{nameof(SerializedFile)} with index {fileIndex} was not found in dependencies");
				}

				FileIdentifier identifier = Metadata.Externals[fileIndex];
				file = Collection.FindSerializedFile(identifier.GetFilePath());
			}

			if (file == null)
			{
				if (isSafe)
				{
					return null;
				}
				throw new Exception($"{nameof(SerializedFile)} with index {fileIndex} was not found in collection");
			}

			Object asset = file.FindAsset(pathID);
			if (asset == null)
			{
				if (isSafe)
				{
					return null;
				}
				throw new Exception($"Object with path ID {pathID} was not found");
			}
			return asset;
		}

		private void ReadAsset(AssetReader reader, ref ObjectInfo info)
		{
			AssetInfo assetInfo = new AssetInfo(this, info.FileID, info.ClassID);
			Object asset = ReadAsset(reader, assetInfo, Header.DataOffset + info.ByteStart, info.ByteSize);
			if (asset != null)
			{
				AddAsset(info.FileID, asset);
			}
		}

		private Object ReadAsset(AssetReader reader, AssetInfo assetInfo, long offset, int size)
		{
			Object asset = Collection.AssetFactory.CreateAsset(assetInfo);
			if (asset == null)
			{
				return null;
			}

			reader.BaseStream.Position = offset;
#if !DEBUG
			try
#endif
			{
				asset.Read(reader);
			}
#if !DEBUG
			catch (Exception ex)
			{
					throw new SerializedFileException($"Error during reading of asset type {asset.ClassID}", ex, Version, Platform, asset.ClassID, Name, FilePath);
			}
#endif
			long read = reader.BaseStream.Position - offset;
			if (read != size)
			{
				throw new SerializedFileException($"Read {read} but expected {size} for asset type {asset.ClassID}", Version, Platform, asset.ClassID, Name, FilePath);
			}
			return asset;
		}

		private void UpdateFileVersion()
		{
			if (!SerializedFileMetadata.HasSignature(Header.Version) && BuildSettings.HasVersion(Version))
			{
				foreach (Object asset in FetchAssets())
				{
					if (asset.ClassID == ClassIDType.BuildSettings)
					{
						BuildSettings settings = (BuildSettings)asset;
						Metadata.UnityVersion = Version.Parse(settings.Version);
						return;
					}
				}
			}
		}

		private void AddAsset(long pathID, Object asset)
		{
			m_assets.Add(pathID, asset);
		}

		public EndianType GetEndianType()
		{
			bool swapEndianess = SerializedFileHeader.HasEndianess(Header.Version) ? Header.Endianess : Metadata.SwapEndianess;
			return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
		}

		public string Name { get; }
		public string NameOrigin { get; }
		public string FilePath { get; }
		public SerializedFileHeader Header { get; }
		public SerializedFileMetadata Metadata { get; }
		public AssetLayout Layout { get; }
		public Version Version => Layout.Info.Version;
		public Platform Platform => Layout.Info.Platform;
		public TransferInstructionFlags Flags => Layout.Info.Flags;

		public IFileCollection Collection { get; }
		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Externals;

		private readonly Dictionary<long, Object> m_assets = new Dictionary<long, Object>();
		private readonly Dictionary<long, int> m_assetEntryLookup = new Dictionary<long, int>();
	}
}
