using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets, .unity3d, but may also have no extension at all
	/// </summary>
	internal class SerializedFile : ISerializedFile
	{
		public SerializedFile(IFileCollection collection, string filePath, string fileName):
			this(collection, filePath, fileName, GetCompiledTransferFlag())
		{
		}

		public SerializedFile(IFileCollection collection, string filePath, string fileName, TransferInstructionFlags flags)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(nameof(fileName));
			}

			Collection = collection;
			FilePath = filePath;
			Name = fileName.ToLower();
			Flags = flags;

			Header = new SerializedFileHeader(Name);
			Metadata = new SerializedFileMetadata(Name);
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

		public void Load(string assetPath, Action<string> requestDependencyCallback)
		{
			if (!FileMultiStream.Exists(assetPath))
			{
				throw new Exception($"Asset at path '{assetPath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(assetPath))
			{
				Read(stream, requestDependencyCallback);
				if (stream.Position != stream.Length)
				{
					//throw new Exception($"Read {read} but expected {m_length}");
				}
			}
		}

		public void Read(byte[] buffer, Action<string> requestDependencyCallback)
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

		public void Read(Stream baseStream, Action<string> requestDependencyCallback)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long startPosition = baseStream.Position;
				Header.Read(stream);

				stream.EndianType = Header.Endianess ? EndianType.BigEndian : EndianType.LittleEndian;
				if (IsTableAtTheEnd(Header.Generation))
				{
					stream.BaseStream.Position = startPosition + Header.FileSize - Header.MetadataSize;
					stream.BaseStream.Position++;
				}
				
				using (SerializedFileStream fileStream = new SerializedFileStream(stream, Header.Generation))
				{
					Metadata.Read(fileStream);
				}

				foreach (FileIdentifier dependency in Dependencies)
				{
					requestDependencyCallback?.Invoke(dependency.FilePathFixed);
				}

				if (RTTIClassHierarchyDescriptor.IsReadSignature(Header.Generation))
				{
					ReadAssets(stream, startPosition);
				}
				else
				{
					Logger.Log(LogType.Warning, LogCategory.Import, $"Can't determine file version for generation {Header.Generation} for file '{Name}'");
					string[] versions = GetGenerationVersions(Header.Generation);
					for (int i = 0; i < versions.Length; i++)
					{
						string version = versions[i];
						Logger.Log(LogType.Debug, LogCategory.Import, $"Try parse {nameof(SerializedFile)} as {version} version");
						Version.Parse(version);
						try
						{
							ReadAssets(stream, startPosition);
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

		public Object GetObject(int fileIndex, long pathID)
		{
			return FindObject(fileIndex, pathID, false);
		}

		public Object GetObject(long pathID)
		{
			Object asset = FindObject(pathID);
			if(asset == null)
			{
				throw new Exception($"Object with path ID {pathID} wasn't found");
			}

			return asset;
		}

		public Object FindObject(int fileIndex, long pathID)
		{
			return FindObject(fileIndex, pathID, true);
		}

		public Object FindObject(long pathID)
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

		private Object FindObject(int fileIndex, long pathID, bool isSafe)
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

			Object @object = file.FindObject(pathID);
			if (@object == null)
			{
				if(isSafe)
				{
					return null;
				}
				throw new Exception($"Object with path ID {pathID} was not found");
			}
			return @object;
		}

		private void ReadAssets(EndianStream stream, long startPosition)
		{
			m_assets.Clear();
			HashSet<long> preloaded = new HashSet<long>();
			using (AssetStream ustream = new AssetStream(stream.BaseStream, Version, Platform, Flags))
			{
				if(SerializedFileMetadata.IsReadPreload(Header.Generation))
				{
					foreach (ObjectPtr ptr in Metadata.Preloads)
					{
						if (ptr.FileID == 0)
						{
							AssetEntry info = Metadata.Objects[ptr.PathID];
							ReadAsset(ustream, info, startPosition);
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
							ReadAsset(ustream, infoPair.Value, startPosition);
							preloaded.Add(infoPair.Key);
						}
					}
				}

				foreach (AssetEntry info in Metadata.Objects.Values)
				{
					if (!preloaded.Contains(info.PathID))
					{
						ReadAsset(ustream, info, startPosition);
					}
				}
			}
		}

		private void ReadAsset(AssetStream stream, AssetEntry info, long startPosition)
		{
			long pathID = info.PathID;
			ClassIDType classID = AssetEntryToClassIDType(info);
			AssetInfo assetInfo = new AssetInfo(this, pathID, classID);
			Object asset = ReadAsset(stream, assetInfo, startPosition + Header.DataOffset + info.DataOffset, info.DataSize);
			if (asset != null)
			{
				AddAsset(pathID, asset);
			}
		}

		private Object ReadAsset(AssetStream stream, AssetInfo assetInfo, long offset, int size)
		{
			Object asset = Collection.AssetFactory.CreateAsset(assetInfo);
			if(asset == null)
			{
				return null;
			}

			stream.BaseStream.Position = offset;
			if (Config.IsGenerateGUIDByContent)
			{
				byte[] data = stream.ReadBytes(size);
				asset.Read(data);

				using (MD5 md5 = MD5.Create())
				{
					byte[] md5Hash = md5.ComputeHash(data);
					assetInfo.GUID = new EngineGUID(md5Hash);
				}
			}
			else
			{
				stream.AlignPosition = offset;
				asset.Read(stream);
				long read = stream.BaseStream.Position - offset;
				if (read != size)
				{
					throw new Exception($"Read {read} but expected {size} for asset type {asset.ClassID}");
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

		public IReadOnlyCollection<Object> Assets => m_assets.Values;
		public IFileCollection Collection { get; }
		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Dependencies;

		private readonly Dictionary<long, Object> m_assets = new Dictionary<long, Object>();
	}
}
