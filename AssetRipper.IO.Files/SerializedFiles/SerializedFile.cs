using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.MultiFile;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.Utils;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.IO.Files.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile
	{
		public string Name { get; }
		public string NameOrigin { get; }
		public string FilePath { get; }
		public SerializedFileHeader Header { get; }
		public SerializedFileMetadata Metadata { get; }
		public UnityVersion Version { get; set; }
		public BuildTarget Platform { get; set; }

		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Externals;
		private readonly Dictionary<long, int> m_assetEntryLookup = new();
		internal SerializedFile(SerializedFileScheme scheme)
		{
			FilePath = scheme.FilePath;
			NameOrigin = scheme.Name;
			Name = FilenameUtils.FixFileIdentifier(scheme.Name);

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

		public ObjectInfo GetAssetEntry(long pathID)
		{
			return Metadata.Object[m_assetEntryLookup[pathID]];
		}

		public override string ToString()
		{
			return Name;
		}

		public EndianType GetEndianType()
		{
			bool swapEndianess = SerializedFileHeader.HasEndianess(Header.Version) ? Header.Endianess : Metadata.SwapEndianess;
			return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
		}
	}
}
