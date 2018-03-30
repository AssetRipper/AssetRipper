using System.Collections.Generic;

namespace UtinyRipper.SerializedFiles
{
	internal class SerializedFileMetadata : ISerializedFileReadable
	{
		public SerializedFileMetadata(string name)
		{
			Hierarchy = new RTTIClassHierarchyDescriptor(name);
		}

		/// <summary>
		/// 5.0.0Unk0 and greater
		/// </summary>
		public static bool IsReadAddresses(FileGeneration generation)
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

		public void Read(SerializedFileStream stream)
		{
			Hierarchy.Read(stream);

			int count = stream.ReadInt32();
			m_objects = new Dictionary<long, ObjectInfo>(count);
			for (int i = 0; i < count; i++)
			{
				ObjectInfo objectInfo = new ObjectInfo();
				objectInfo.Read(stream);
				m_objects.Add(objectInfo.PathID, objectInfo);
			}

			if (IsReadAddresses(stream.Generation))
			{
				m_addresses = stream.ReadArray<ObjectPtr>();
			}
			m_dependencies = stream.ReadArray<FileIdentifier>();
			if (IsReadUnknown(stream.Generation))
			{
				Unknown = stream.ReadStringZeroTerm();
			}
		}

		public RTTIClassHierarchyDescriptor Hierarchy { get; }
		public IReadOnlyDictionary<long, ObjectInfo> Objects => m_objects;
		public IReadOnlyList<ObjectPtr> Addresses => m_addresses;
		public IReadOnlyList<FileIdentifier> Dependencies => m_dependencies;
		public string Unknown { get; private set; }
		
		private Dictionary<long, ObjectInfo> m_objects;
		private ObjectPtr[] m_addresses;
		private FileIdentifier[] m_dependencies;
	}
}
