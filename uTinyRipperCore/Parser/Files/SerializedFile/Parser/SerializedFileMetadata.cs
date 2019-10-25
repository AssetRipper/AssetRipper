using System.Collections.Generic;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedFileMetadata : ISerializedFileReadable
	{
		public SerializedFileMetadata(string name)
		{
			Hierarchy = new RTTIClassHierarchyDescriptor(name);
		}

		/// <summary>
		/// 5.0.0Unk0 and greater
		/// </summary>
		public static bool IsReadPreload(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_500aunk;
		}
		/// <summary>
		/// 1.2.0 and greater
		/// </summary>
		public static bool IsReadUnknown(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_120_200;
		}

		public void Read(SerializedFileReader reader)
		{
			Hierarchy.Read(reader);

			int count = reader.ReadInt32();
			Dictionary<long, AssetEntry> entries = new Dictionary<long, AssetEntry>(count);
			for (int i = 0; i < count; i++)
			{
				AssetEntry entry = new AssetEntry();
				entry.Read(reader, Hierarchy);
				entries.Add(entry.PathID, entry);
			}
			Entries = entries;

			if (IsReadPreload(reader.Generation))
			{
				Preloads = reader.ReadSerializedArray<ObjectPtr>();
			}
			Dependencies = reader.ReadSerializedArray<FileIdentifier>();
			if (IsReadUnknown(reader.Generation))
			{
				Unknown = reader.ReadStringZeroTerm();
			}
		}

		public RTTIClassHierarchyDescriptor Hierarchy { get; }
		public IReadOnlyDictionary<long, AssetEntry> Entries { get; private set; }
		public IReadOnlyList<ObjectPtr> Preloads { get; private set; }
		public IReadOnlyList<FileIdentifier> Dependencies { get; private set; }
		public string Unknown { get; private set; }

		public const int MetadataMinSize = RTTIClassHierarchyDescriptor.HierarchyMinSize + 12;
	}
}
