using System;
using System.Collections.Generic;

namespace UtinyRipper.BundleFiles
{
	/// <summary>
	/// Metadata about bundle's block or chunk
	/// </summary>
	public class BundleMetadata : Metadata<BundleFileEntry>
	{
		internal BundleMetadata(string filePath)
		{
			if(string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(filePath);
			}
			m_filePath = filePath;
		}

		internal BundleMetadata(string filePath, IReadOnlyList<BundleFileEntry> entries):
			this(filePath)
		{
			m_entries = entries;
		}

		public void ReadPre530(EndianReader reader)
		{
			SmartStream dataStream = (SmartStream)reader.BaseStream;
			long basePosition = reader.BaseStream.Position;

			int count = reader.ReadInt32();
			BundleFileEntry[] entries = new BundleFileEntry[count];
			for (int i = 0; i < count; i++)
			{
				string name = reader.ReadStringZeroTerm();
				int offset = reader.ReadInt32();
				int size = reader.ReadInt32();

				long globalOffset = basePosition + offset;
				BundleFileEntry entry = new BundleFileEntry(dataStream, m_filePath, name, globalOffset, size);
				entries[i] = entry;
			}
			m_entries = entries;
		}

		public void Read530(EndianReader reader, SmartStream dataStream)
		{
			int count = reader.ReadInt32();
			BundleFileEntry[] entries = new BundleFileEntry[count];
			for (int i = 0; i < count; i++)
			{
				long offset = reader.ReadInt64();
				long size = reader.ReadInt64();
				int blobIndex = reader.ReadInt32();
				string name = reader.ReadStringZeroTerm();
				
				BundleFileEntry entry = new BundleFileEntry(dataStream, m_filePath, name, offset, size);
				entries[i] = entry;
			}
			m_entries = entries;
		}

		public override IReadOnlyList<BundleFileEntry> Entries => m_entries;

		private readonly string m_filePath;

		private IReadOnlyList<BundleFileEntry> m_entries;
	}
}
