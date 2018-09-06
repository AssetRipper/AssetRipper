using System.Collections.Generic;

namespace UtinyRipper.SerializedFiles
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
			m_objects = new Dictionary<long, AssetEntry>(count);
			for (int i = 0; i < count; i++)
			{
				AssetEntry objectInfo = new AssetEntry();
				objectInfo.Read(reader);
				m_objects.Add(objectInfo.PathID, objectInfo);
			}

			if (IsReadPreload(reader.Generation))
			{
				m_preloads = reader.ReadArray<ObjectPtr>();
			}
			m_dependencies = reader.ReadArray<FileIdentifier>();
			if (IsReadUnknown(reader.Generation))
			{
				Unknown = reader.ReadStringZeroTerm();
			}
		}

		public RTTIClassHierarchyDescriptor Hierarchy { get; }
		public IReadOnlyDictionary<long, AssetEntry> Objects => m_objects;
		public IReadOnlyList<ObjectPtr> Preloads => m_preloads;
		public IReadOnlyList<FileIdentifier> Dependencies => m_dependencies;
		public string Unknown { get; private set; }
		
		private Dictionary<long, AssetEntry> m_objects;
		private ObjectPtr[] m_preloads;
		private FileIdentifier[] m_dependencies;
	}
}
