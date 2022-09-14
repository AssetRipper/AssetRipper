using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.IO.Smart;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure;
using AssetRipper.Core.VersionHandling;
using AssetRipper.IO.Endian;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Parser.Files.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile : ISerializedFile
	{
		public string Name { get; }
		public string NameOrigin { get; }
		public string FilePath { get; }
		public SerializedFileHeader Header { get; }
		public SerializedFileMetadata Metadata { get; }
		public LayoutInfo Layout { get; }
		public UnityVersion Version => Layout.Version;
		public BuildTarget Platform => Layout.Platform;
		public TransferInstructionFlags Flags => Layout.Flags;
		public EndianType EndianType
		{
			get
			{
				bool swapEndianess = SerializedFileHeader.HasEndianess(Header.Version) ? Header.Endianess : Metadata.SwapEndianess;
				return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			}
		}

		public IFileCollection Collection { get; }
		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Externals;

		private readonly Dictionary<long, IUnityObjectBase> m_assets = new();
		private readonly Dictionary<long, int> m_assetEntryLookup = new();
		internal SerializedFile(IFileCollection collection, SerializedFileScheme scheme)
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

		public static bool IsSerializedFile(string filePath) => IsSerializedFile(MultiFileStream.OpenRead(filePath));
		public static bool IsSerializedFile(byte[] buffer, int offset, int size) => IsSerializedFile(new MemoryStream(buffer, offset, size, false));
		public static bool IsSerializedFile(Stream stream)
		{
			using EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
			return SerializedFileHeader.IsSerializedFileHeader(reader, stream.Length);
		}

		public static SerializedFileScheme LoadScheme(string filePath)
		{
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			using SmartStream fileStream = SmartStream.OpenRead(filePath);
			return ReadScheme(fileStream, filePath, fileName);
		}

		public static SerializedFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			return SerializedFileScheme.ReadSceme(buffer, filePath, fileName);
		}

		public static SerializedFileScheme ReadScheme(SmartStream stream, string filePath, string fileName)
		{
			return SerializedFileScheme.ReadSceme(stream, filePath, fileName);
		}

		private static LayoutInfo GetLayout(IFileCollection collection, SerializedFileScheme scheme, string name)
		{
			if (!SerializedFileMetadata.HasPlatform(scheme.Header.Version))
			{
				return collection.Layout;
			}
			if (FilenameUtils.IsDefaultResource(name))
			{
				return collection.Layout;
			}

			return new LayoutInfo(scheme.Metadata.UnityVersion, scheme.Metadata.TargetPlatform, scheme.Flags);
		}

		public IUnityObjectBase GetAsset(long pathID)
		{
			return TryGetAsset(pathID) ?? throw new Exception($"Object with path ID {pathID} wasn't found");
		}

		public IUnityObjectBase GetAsset(int fileIndex, long pathID)
		{
			return FindAsset(fileIndex, pathID, false) ?? throw new Exception($"Object with file ID {fileIndex} path ID {pathID} wasn't found");
		}

		public IUnityObjectBase? TryGetAsset(long pathID)
		{
			m_assets.TryGetValue(pathID, out IUnityObjectBase? asset);
			return asset;
		}

		public IUnityObjectBase? TryGetAsset(int fileIndex, long pathID)
		{
			return FindAsset(fileIndex, pathID, true);
		}

		public ObjectInfo GetAssetEntry(long pathID)
		{
			return Metadata.Object[m_assetEntryLookup[pathID]];
		}

		public PPtr<T> CreatePPtr<T>(T asset) where T : IUnityObjectBase
		{
			if (asset.SerializedFile == this)
			{
				return new PPtr<T>(0, asset.PathID);
			}

			for (int i = 0; i < Metadata.Externals.Length; i++)
			{
				FileIdentifier identifier = Metadata.Externals[i];
				ISerializedFile? file = Collection.FindSerializedFile(identifier.GetFilePath());
				if (asset.SerializedFile == file)
				{
					return new PPtr<T>(i + 1, asset.PathID);
				}
			}

			throw new Exception("Asset doesn't belong to this serialized file or its dependencies");
		}

		public IEnumerable<IUnityObjectBase> FetchAssets()
		{
			return m_assets.Values;
		}

		public override string ToString()
		{
			return Name;
		}

		internal void ReadData(Stream stream)
		{
			using AssetReader assetReader = new AssetReader(stream, EndianType, Layout);
			if (SerializedFileMetadata.HasScriptTypes(Header.Version))
			{
				foreach (LocalSerializedObjectIdentifier ptr in Metadata.ScriptTypes)
				{
					if (ptr.LocalSerializedFileIndex == 0)
					{
						int index = m_assetEntryLookup[ptr.LocalIdentifierInFile];
						ReadAsset(assetReader, Metadata.Object[index]);
					}
				}
			}

			for (int i = 0; i < Metadata.Object.Length; i++)
			{
				if (Metadata.Object[i].ClassID == ClassIDType.MonoScript)
				{
					if (!m_assets.ContainsKey(Metadata.Object[i].FileID))
					{
						ReadAsset(assetReader, Metadata.Object[i]);
					}
				}
			}

			for (int i = 0; i < Metadata.Object.Length; i++)
			{
				if (!m_assets.ContainsKey(Metadata.Object[i].FileID))
				{
					ReadAsset(assetReader, Metadata.Object[i]);
				}
			}
		}

		private IUnityObjectBase? FindAsset(int fileIndex, long pathID, bool isSafe)
		{
			ISerializedFile? file;
			if (fileIndex == 0)
			{
				file = this;
			}
			else if (fileIndex < 0)
			{
				if (isSafe)
				{
					Logger.Error($"File index cannot be negative: {fileIndex}");
					return null;
				}
				else
				{
					throw new ArgumentOutOfRangeException(nameof(fileIndex), $"File index cannot be negative: {fileIndex}");
				}
			}
			else
			{
				fileIndex--;
				if (fileIndex >= Metadata.Externals.Length)
				{
					if (isSafe)
					{
						Logger.Error($"{nameof(SerializedFile)} with index {fileIndex} was not found in dependencies");
						return null;
					}
					else
					{
						throw new ArgumentException($"{nameof(SerializedFile)} with index {fileIndex} was not found in dependencies", nameof(fileIndex));
					}
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

			IUnityObjectBase? asset = file.TryGetAsset(pathID);
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

		private void ReadAsset(AssetReader reader, ObjectInfo info)
		{
			AssetInfo assetInfo = new AssetInfo(this, info.FileID, info.ClassID);
			IUnityObjectBase asset = ReadAsset(reader, assetInfo, Header.DataOffset + info.ByteStart, info.ByteSize);
			AddAsset(info.FileID, asset);
		}

		private IUnityObjectBase ReadAsset(AssetReader reader, AssetInfo assetInfo, long offset, int size)
		{
			IUnityObjectBase? asset;
			try
			{
				asset = VersionManager.AssetFactory.CreateAsset(assetInfo, Version);
			}
			catch (TypeLoadException typeLoadException)
			{
				Logger.Error($"Could not load {typeLoadException.TypeName} : {typeLoadException.Message}");
				asset = null;
			}

			bool replaceWithUnreadableObject = false;
			reader.AdjustableStream.SetPositionBoundaries(offset, offset + size, offset);
			if (asset is null)
			{
				UnknownObject unknownObject = new UnknownObject(assetInfo);
				unknownObject.Read(reader, size);
				asset = unknownObject;
			}
			else
			{
				try
				{
					asset.Read(reader);
				}
				catch (Exception ex)
				{
					replaceWithUnreadableObject = true;
					Logger.Error($"Error during reading of asset type {assetInfo.ClassID}. V: {Version} P: {Platform} N: {Name} Path: {FilePath}", ex);
				}
			}

			long read = reader.BaseStream.Position - offset;
			if (!replaceWithUnreadableObject && read != size)
			{
				if (asset is IMonoBehaviourBase monoBehaviour && monoBehaviour.Structure == null)
				{
					reader.BaseStream.Position = offset + size;
				}
				else
				{
					replaceWithUnreadableObject = true;
					Logger.Error($"Read {read} but expected {size} for asset type {assetInfo.ClassID}. V: {Version} P: {Platform} N: {Name} Path: {FilePath}");
				}
			}

			if (replaceWithUnreadableObject)
			{
				reader.AdjustableStream.Position = offset;
				UnreadableObject unreadable = new UnreadableObject(assetInfo);
				unreadable.Read(reader, size);
				unreadable.NameString = asset is IHasNameString hasName ? hasName.NameString : asset.GetType().Name;
				asset = unreadable;
			}

			reader.AdjustableStream.ResetPositionBoundaries();
			return asset;
		}
		/*
		private void UpdateFileVersion()
		{
			if (!SerializedFileMetadata.HasSignature(Header.Version))
			{
				foreach (IUnityObjectBase asset in FetchAssets())
				{
					if (asset is IBuildSettings settings && settings.Version != null)
					{
						Metadata.UnityVersion = UnityVersion.Parse(settings.Version);
						return;
					}
				}
			}
		}*/

		private void AddAsset(long pathID, IUnityObjectBase asset)
		{
			m_assets.Add(pathID, asset);
		}
	}
}
