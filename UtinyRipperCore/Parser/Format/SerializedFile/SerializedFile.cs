using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets, .unity3d, but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile : ISerializedFile
	{
		public struct Parameters
		{
			public IFileCollection FileCollection { get; set; }
			public IAssemblyManager AssemblyManager { get; set; }
			public string FilePath { get; set; }
			public string Name { get; set; }
			public Action<string> DependencyCallback { get; set; }
			public TransferInstructionFlags Flags { get; set; }
		}

		internal SerializedFile(IFileCollection collection, IAssemblyManager manager, SerializedFileScheme scheme)
		{
			if (scheme == null)
			{
				throw new ArgumentNullException(nameof(scheme));
			}
#warning TODO:
		}

		private SerializedFile(IFileCollection collection, IAssemblyManager manager, string filePath, string name, TransferInstructionFlags flags)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			Collection = collection;
			AssemblyManager = manager;
			FilePath = filePath;
			Name = FilenameUtils.FixFileIdentifier(name);
			Flags = flags;

			Header = new SerializedFileHeader(Name);
			Metadata = new SerializedFileMetadata(Name);
		}

		public static SerializedFile Load(Parameters pars)
		{
			SerializedFile file = new SerializedFile(pars.FileCollection, pars.AssemblyManager, pars.FilePath, pars.Name, pars.Flags);
			file.Load(pars.DependencyCallback);
			return file;
		}

		public static SerializedFile Read(Stream stream, Parameters pars)
		{
			SerializedFile file = new SerializedFile(pars.FileCollection, pars.AssemblyManager, pars.FilePath, pars.Name, pars.Flags);
			file.Read(stream, pars.DependencyCallback);
			return file;
		}

		private static TransferInstructionFlags GetEditorTransferFlag()
		{
			return TransferInstructionFlags.NoTransferInstructionFlags;
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsTableAtTheEnd(FileGeneration generation)
		{
			return generation <= FileGeneration.FG_300_342;
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
			return Metadata.Objects[pathID];
		}

		public ClassIDType GetClassID(long pathID)
		{
			AssetEntry info = Metadata.Objects[pathID];
			if (AssetEntry.IsReadTypeIndex(Header.Generation))
			{
				return Metadata.Hierarchy.Types[info.TypeIndex].ClassID;
			}
			else
			{
				return info.ClassID;
			}
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
		
		private void Load(Action<string> dependencyCallback)
		{
			if (!FileMultiStream.Exists(FilePath))
			{
				throw new Exception($"Serialized file at path '{FilePath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(FilePath))
			{
				Read(stream, dependencyCallback);
				if (stream.Position != stream.Length)
				{
					//throw new Exception($"Read {read} but expected {m_length}");
				}
			}
		}

		private void Read(Stream stream, Action<string> dependencyCallback)
		{
			long startPosition = stream.Position;
			using (EndianReader reader = new EndianReader(stream, stream.Position, EndianType.BigEndian))
			{
				Header.Read(reader);
			}
			
			EndianType endianess = Header.SwapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			using (EndianReader reader = new EndianReader(stream, stream.Position, endianess))
			{
				if (IsTableAtTheEnd(Header.Generation))
				{
					reader.BaseStream.Position = startPosition + Header.FileSize - Header.MetadataSize;
					reader.BaseStream.Position++;
				}

				using (SerializedFileReader fileReader = new SerializedFileReader(reader, Header.Generation))
				{
					Metadata.Read(fileReader);
				}

#warning TEMP HACK
				Flags = Platform == Platform.NoTarget ? TransferInstructionFlags.NoTransferInstructionFlags : Flags;
				Flags |= Header.SwapEndianess ? TransferInstructionFlags.SwapEndianess : TransferInstructionFlags.NoTransferInstructionFlags;

				foreach (FileIdentifier dependency in Dependencies)
				{
					dependencyCallback?.Invoke(dependency.FilePath);
				}

				if (RTTIClassHierarchyDescriptor.IsReadSignature(Header.Generation))
				{
					ReadAssets(reader, startPosition);
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
							ReadAssets(reader, startPosition);
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

		private void ReadAssets(EndianReader reader, long startPosition)
		{
			HashSet<long> preloaded = new HashSet<long>();
			if (SerializedFileMetadata.IsReadPreload(Header.Generation))
			{
				foreach (ObjectPtr ptr in Metadata.Preloads)
				{
					if (ptr.FileID == 0)
					{
						AssetEntry info = Metadata.Objects[ptr.PathID];
						ReadAsset(reader, info, startPosition);
						preloaded.Add(ptr.PathID);
					}
				}
			}

			foreach (KeyValuePair<long, AssetEntry> infoPair in Metadata.Objects)
			{
				ClassIDType classID = AssetEntryToClassIDType(infoPair.Value);
				if (classID == ClassIDType.MonoScript)
				{
					if (!preloaded.Contains(infoPair.Key))
					{
						ReadAsset(reader, infoPair.Value, startPosition);
						preloaded.Add(infoPair.Key);
					}
				}
			}

			foreach (AssetEntry info in Metadata.Objects.Values)
			{
				if (!preloaded.Contains(info.PathID))
				{
					ReadAsset(reader, info, startPosition);
				}
			}
		}

		private void ReadAsset(EndianReader reader, AssetEntry info, long startPosition)
		{
			long pathID = info.PathID;
			ClassIDType classID = AssetEntryToClassIDType(info);
			AssetInfo assetInfo = new AssetInfo(this, pathID, classID);
			Object asset = ReadAsset(reader, assetInfo, startPosition + Header.DataOffset + info.DataOffset, info.DataSize);
			if (asset != null)
			{
				AddAsset(pathID, asset);
			}
		}

		private Object ReadAsset(EndianReader reader, AssetInfo assetInfo, long offset, int size)
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
				catch
				{
					Logger.Instance.Log(LogType.Error, LogCategory.General, $"Version[{Version}] '{Name}'");
					throw;
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
				using (AssetReader alignReader = new AssetReader(reader, Version, Platform, Flags))
				{
#if !DEBUG
					try
#endif
					{
						asset.Read(alignReader);
					}
#if !DEBUG
					catch
					{
						Logger.Instance.Log(LogType.Error, LogCategory.General, $"Version[{Version}] '{Name}'");
						throw;
					}
#endif
				}
				long read = reader.BaseStream.Position - offset;
				if (read != size)
				{
					throw new Exception($"Read {read} but expected {size} for asset type {asset.ClassID}. Version[{Version}] '{Name}'");
				}
			}
			return asset;
		}

		private void AddAsset(long pathID, Object asset)
		{
			m_assets.Add(pathID, asset);
		}

		private ClassIDType AssetEntryToClassIDType(AssetEntry info)
		{
			if (AssetEntry.IsReadTypeIndex(Header.Generation))
			{
				RTTIBaseClassDescriptor typemeta = Metadata.Hierarchy.Types[info.TypeIndex];
				return typemeta.ClassID;
			}
			else
			{
				return info.ClassID;
			}
		}

		public string Name { get; }
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
