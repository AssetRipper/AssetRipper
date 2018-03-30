using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.BundleFiles
{
	/// <summary>
	/// Metadata about bundle's block or chunk
	/// </summary>
	internal class BundleMetadata : FileData<BundleFileEntry>
	{
		public BundleMetadata(Stream stream, bool isClosable):
			base(stream, isClosable)
		{
		}

		public BundleMetadata(Stream stream, bool isClosable, IReadOnlyList<BundleFileEntry> entries):
			this(stream, isClosable)
		{
			m_entries = entries;
		}

		public void ReadPre530Metadata(EndianStream stream)
		{
			if (m_stream != stream.BaseStream)
			{
				throw new ArgumentException("Stream doesn't match", nameof(stream));
			}

			long basePosition = stream.BaseStream.Position;

			int count = stream.ReadInt32();
			BundleFileEntry[] entries = new BundleFileEntry[count];
			for (int i = 0; i < count; i++)
			{
				string name = stream.ReadStringZeroTerm();
				int offset = stream.ReadInt32();
				int size = stream.ReadInt32();

				long globalOffset = basePosition + offset;
				BundleFileEntry entry = new BundleFileEntry(m_stream, name, globalOffset, size);
				entries[i] = entry;
			}
			m_entries = entries;
		}

		public void Read530Metadata(EndianStream stream, long basePosition)
		{
			int count = stream.ReadInt32();
			BundleFileEntry[] entries = new BundleFileEntry[count];
			for (int i = 0; i < count; i++)
			{
				long offset = stream.ReadInt64();
				long size = stream.ReadInt64();
				int blobIndex = stream.ReadInt32();
				string name = stream.ReadStringZeroTerm();
				
				long globalOffset = basePosition + offset;
				BundleFileEntry entry = new BundleFileEntry(m_stream, name, globalOffset, size);
				entries[i] = entry;
			}
			m_entries = entries;
		}

		public IReadOnlyList<BundleFileEntry> Entries => m_entries;
	}
}
