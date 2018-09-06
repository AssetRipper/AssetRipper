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
			Flags = flags == TransferInstructionFlags.NoTransferInstructionFlags ? GetCompiledTransferFlag() : flags;

			Header = new SerializedFileHeader(Name);
			Metadata = new SerializedFileMetadata(Name);
		}

		public static SerializedFile Load(Parameters pars)
		{
			SerializedFile file = new SerializedFile(pars.FileCollection, pars.AssemblyManager, pars.FilePath, pars.Name, pars.Flags);
			file.Load(pars.DependencyCallback);
			return file;
		}

		public static SerializedFile Read(byte[] data, Parameters pars)
		{
			SerializedFile file = new SerializedFile(pars.FileCollection, pars.AssemblyManager, pars.FilePath, pars.Name, pars.Flags);
			file.Read(data, pars.DependencyCallback);
			return file;
		}

		public static SerializedFile Read(Stream stream, Parameters pars)
		{
			SerializedFile file = new SerializedFile(pars.FileCollection, pars.AssemblyManager, pars.FilePath, pars.Name, pars.Flags);
			file.Read(stream, pars.DependencyCallback);
			return file;
		}

		private static TransferInstructionFlags GetCompiledTransferFlag()
		{
			return TransferInstructionFlags.Unknown1 | TransferInstructionFlags.SerializeGameRelease;
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsTableAtTheEnd(FileGeneration generation)
		{
			return generation <= FileGeneration.FG_300_342;
		}

		public Object GetAsset(int fileIndex, long pathID)
		{
			return FindAsset(fileIndex, pathID, false);
		}

		public Object GetAsset(long pathID)
		{
			Object asset = FindAsset(pathID);
			if(asset == null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}

			return asset;
		}

		public Object FindAsset(int fileIndex, long pathID)
		{
			return FindAsset(fileIndex, pathID, true);
		}

		public Object FindAsset(long pathID)
		{
			m_assets.TryGetValue(pathID, out Object asset);
			return asset;
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

		private void Read(byte[] buffer, Action<string> requestDependencyCallback)
		{
			using (MemoryStream memStream = new MemoryStream(buffer))
			{
				Read(memStream, requestDependencyCallback);
				if (memStream.Position != buffer.Length)
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

			EndianType endianess = Header.Endianess ? EndianType.BigEndian : EndianType.LittleEndian;
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
						Version.Parse(version);
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
				if (fileIndex >= Metadata.Dependencies.Count)
				{
					throw new Exception($"{nameof(SerializedFile)} with index {fileIndex} was not found in dependencies");
				}

				FileIdentifier fileRef = Metadata.Dependencies[fileIndex];
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
			using (AssetReader assetReader = new AssetReader(reader.BaseStream, Version, Platform, Flags))
			{
				if(SerializedFileMetadata.IsReadPreload(Header.Generation))
				{
					foreach (ObjectPtr ptr in Metadata.Preloads)
					{
						if (ptr.FileID == 0)
						{
							AssetEntry info = Metadata.Objects[ptr.PathID];
							ReadAsset(assetReader, info, startPosition);
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
							ReadAsset(assetReader, infoPair.Value, startPosition);
							preloaded.Add(infoPair.Key);
						}
					}
				}

				foreach (AssetEntry info in Metadata.Objects.Values)
				{
					if (!preloaded.Contains(info.PathID))
					{
						ReadAsset(assetReader, info, startPosition);
					}
				}
			}
		}

		private void ReadAsset(AssetReader reader, AssetEntry info, long startPosition)
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
				catch
				{
					Logger.Instance.Log(LogType.Error, LogCategory.General, $"Version[{reader.Version}] '{Name}'");
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
				using (AssetReader alignReader = new AssetReader(reader, offset))
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
						Logger.Instance.Log(LogType.Error, LogCategory.General, $"Version[{reader.Version}] '{Name}'");
						throw;
					}
#endif
				}
				long read = reader.BaseStream.Position - offset;
				if (read != size)
				{
					throw new Exception($"Read {read} but expected {size} for asset type {asset.ClassID}. Version[{reader.Version}] '{Name}'");
				}
			}
			return asset;
		}

		private void AddAsset(long pathID, Object asset)
		{
			if(!IsScene)
			{
				// save IsScene value for optimization purpose
				if(asset.ClassID.IsSceneSettings())
				{
					IsScene = true;
				}
			}
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
		public TransferInstructionFlags Flags { get; }
		
		public bool IsScene { get; private set; }

		public IEnumerable<Object> Assets => m_assets.Values;
		public IFileCollection Collection { get; }
		public IAssemblyManager AssemblyManager { get; }
		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Dependencies;

		private readonly Dictionary<long, Object> m_assets = new Dictionary<long, Object>();
	}
}
