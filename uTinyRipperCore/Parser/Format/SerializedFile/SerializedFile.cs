using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using uTinyRipper.Assembly;
using uTinyRipper.Classes;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets, .unity3d, but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile : ISerializedFile
	{
		internal SerializedFile(IFileCollection collection, IAssemblyManager manager, SerializedFileScheme scheme)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}
			if (scheme == null)
			{
				throw new ArgumentNullException(nameof(scheme));
			}

			Collection = collection;
			AssemblyManager = manager;
			FilePath = scheme.FilePath;
			NameOrigin = scheme.Name;
			Name = FilenameUtils.FixFileIdentifier(scheme.Name);
			Flags = scheme.Flags;

			Header = scheme.Header;
			Metadata = scheme.Metadata;
		}

		public static bool IsSerializedFile(string filePath)
		{
			if (!FileMultiStream.Exists(filePath))
			{
				throw new Exception($"Serialized file at '{filePath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(filePath))
			{
				return IsSerializedFile(stream);
			}
		}

		public static bool IsSerializedFile(Stream stream)
		{
			return IsSerializedFile(stream, stream.Position, stream.Length - stream.Position);
		}

		public static bool IsSerializedFile(Stream stream, long offset, long size)
		{
			using (PartialStream bundleStream = new PartialStream(stream, offset, size))
			{
				using (EndianReader reader = new EndianReader(bundleStream, EndianType.BigEndian))
				{
					return SerializedFileHeader.IsSerializedFileHeader(reader);
				}
			}
		}

		public static SerializedFileScheme LoadScheme(string filePath, string fileName)
		{
			if (!FileMultiStream.Exists(filePath))
			{
				throw new Exception($"Serialized file at path '{filePath}' doesn't exist");
			}
			using (SmartStream fileStream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(fileStream, 0, fileStream.Length, filePath, fileName);
			}
		}

		public static SerializedFileScheme LoadScheme(string filePath, string fileName, TransferInstructionFlags flags)
		{
			if (!FileMultiStream.Exists(filePath))
			{
				throw new Exception($"Serialized file at path '{filePath}' doesn't exist");
			}
			using (SmartStream fileStream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(fileStream, 0, fileStream.Length, filePath, fileName, flags);
			}
		}

		public static SerializedFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			return SerializedFileScheme.ReadSceme(stream, offset, size, filePath, fileName);
		}

		public static SerializedFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName, TransferInstructionFlags flags)
		{
			return SerializedFileScheme.ReadSceme(stream, offset, size, filePath, fileName, flags);
		}

		private static string[] GetGenerationVersions(FileGeneration generation)
		{
			if (generation < FileGeneration.FG_120_200)
			{
				return new[] { "1.2.2" };
			}

			switch (generation)
			{
				case FileGeneration.FG_120_200:
					return new[] { "2.0.0", "1.6.0", "1.5.0", "1.2.2" };
				case FileGeneration.FG_210_261:
					return new[] { "2.6.1", "2.6.0", "2.5.1", "2.5.0", "2.1.0", };
				case FileGeneration.FG_300b:
					return new[] { "3.0.0b1" };
				default:
					throw new NotSupportedException();
			}
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

			foreach (FileIdentifier identifier in Dependencies)
			{
				ISerializedFile file = Collection.FindSerializedFile(identifier);
				if (file == null)
				{
					continue;
				}
				foreach(Object asset in file.FetchAssets())
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
					if(namedAsset.ValidName == name)
					{
						return asset;
					}
				}
			}

			foreach (FileIdentifier identifier in Dependencies)
			{
				ISerializedFile file = Collection.FindSerializedFile(identifier);
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

		public AssetEntry GetAssetEntry(long pathID)
		{
			return Metadata.Entries[pathID];
		}

		public ClassIDType GetClassID(long pathID)
		{
			return Metadata.Entries[pathID].ClassID;
		}

		public PPtr<T> CreatePPtr<T>(T asset)
			where T: Object
		{
			if(asset.File == this)
			{
				return new PPtr<T>(0, asset.PathID);
			}

			for (int i = 0; i < Dependencies.Count; i++)
			{
				FileIdentifier identifier = Dependencies[i];
				ISerializedFile file = Collection.FindSerializedFile(identifier);
				if(asset.File == file)
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
		
		internal void Read(EndianReader reader)
		{
			if (RTTIClassHierarchyDescriptor.IsReadSignature(Header.Generation))
			{
				ReadAssets(reader);
			}
			else
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"Can't determine file version for generation {Header.Generation} for file '{Name}'");
				string[] versions = GetGenerationVersions(Header.Generation);
				for (int i = 0; i < versions.Length; i++)
				{
					string version = versions[i];
					Logger.Log(LogType.Debug, LogCategory.Import, $"Try parse {Name} as {version} version");
					Metadata.Hierarchy.Version.Parse(version);
					m_assets.Clear();
					try
					{
						ReadAssets(reader);
						UpdateFileVersion();
						break;
					}
					catch
					{
						Logger.Log(LogType.Debug, LogCategory.Import, "Faild");
						if (i == versions.Length - 1)
						{
							throw;
						}
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
				if (fileIndex >= Dependencies.Count)
				{
					throw new Exception($"{nameof(SerializedFile)} with index {fileIndex} was not found in dependencies");
				}

				FileIdentifier fileRef = Dependencies[fileIndex];
				file = Collection.FindSerializedFile(fileRef);
			}

			if (file == null)
			{
				if(isSafe)
				{
					return null;
				}
				throw new Exception($"{nameof(SerializedFile)} with index {fileIndex} was not found in collection");
			}

			Object asset = file.FindAsset(pathID);
			if (asset == null)
			{
				if(isSafe)
				{
					return null;
				}
				throw new Exception($"Object with path ID {pathID} was not found");
			}
			return asset;
		}

		private void ReadAssets(EndianReader reader)
		{
			HashSet<long> preloaded = new HashSet<long>();
			using (AssetReader assetReader = new AssetReader(reader, Version, Platform, Flags))
			{
				if (SerializedFileMetadata.IsReadPreload(Header.Generation))
				{
					foreach (ObjectPtr ptr in Metadata.Preloads)
					{
						if (ptr.FileID == 0)
						{
							AssetEntry info = Metadata.Entries[ptr.PathID];
							ReadAsset(assetReader, info);
							preloaded.Add(ptr.PathID);
						}
					}
				}

				foreach (KeyValuePair<long, AssetEntry> infoPair in Metadata.Entries)
				{
					if (infoPair.Value.ClassID == ClassIDType.MonoScript)
					{
						if (!preloaded.Contains(infoPair.Key))
						{
							ReadAsset(assetReader, infoPair.Value);
							preloaded.Add(infoPair.Key);
						}
					}
				}

				foreach (AssetEntry info in Metadata.Entries.Values)
				{
					if (!preloaded.Contains(info.PathID))
					{
						ReadAsset(assetReader, info);
					}
				}
			}
		}

		private void ReadAsset(AssetReader reader, AssetEntry info)
		{
			AssetInfo assetInfo = new AssetInfo(this, info.PathID, info.ClassID);
			Object asset = ReadAsset(reader, assetInfo, Header.DataOffset + info.Offset, info.Size);
			if (asset != null)
			{
				AddAsset(info.PathID, asset);
			}
		}

		private Object ReadAsset(AssetReader reader, AssetInfo assetInfo, long offset, int size)
		{
			Object asset = Collection.AssetFactory.CreateAsset(assetInfo);
			if(asset == null)
			{
				return null;
			}

			reader.BaseStream.Position = offset;
			if (Config.IsGenerateGUIDByContent)
			{
				byte[] data = reader.ReadBytes(size);
#if !DEBUG
				try
#endif
				{
					asset.Read(data);
				}
#if !DEBUG
				catch (Exception ex)
				{
					throw new SerializedFileException($"Error during reading asset type {asset.ClassID}", ex, Version, Name, FilePath);
				}
#endif

				using (MD5 md5 = MD5.Create())
				{
					byte[] md5Hash = md5.ComputeHash(data);
					assetInfo.GUID = new EngineGUID(md5Hash);
				}
			}
			else
			{
#if !DEBUG
				try
#endif
				{
					asset.Read(reader);
				}
#if !DEBUG
				catch (Exception ex)
				{
					throw new SerializedFileException($"Error during reading asset type {asset.ClassID}", ex, Version, Name, FilePath);
				}
#endif
				long read = reader.BaseStream.Position - offset;
				if (read != size)
				{
					throw new SerializedFileException($"Read {read} but expected {size} for asset type {asset.ClassID}", Version, Name, FilePath);
				}
			}
			return asset;
		}

		private void UpdateFileVersion()
		{
			if (!RTTIClassHierarchyDescriptor.IsReadSignature(Header.Generation))
			{
				foreach (Object asset in FetchAssets())
				{
					if (asset.ClassID == ClassIDType.BuildSettings)
					{
						BuildSettings settings = (BuildSettings)asset;
						Metadata.Hierarchy.Version.Parse(settings.Version);
						return;
					}
				}
			}
		}

		private void AddAsset(long pathID, Object asset)
		{
			m_assets.Add(pathID, asset);
		}

		public string Name { get; }
		public string NameOrigin { get; }
		public string FilePath { get; }
		public SerializedFileHeader Header { get; }
		public SerializedFileMetadata Metadata { get; }
		public Version Version => Metadata.Hierarchy.Version;
		public Platform Platform => Metadata.Hierarchy.Platform;
		public TransferInstructionFlags Flags { get; private set; }
		
		public IFileCollection Collection { get; }
		public IAssemblyManager AssemblyManager { get; }
		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Dependencies;

		private readonly Dictionary<long, Object> m_assets = new Dictionary<long, Object>();
	}
}
