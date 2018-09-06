using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.BundleFiles
{
	/// <summary>
	/// Metadata about bundle's block or chunk
	/// </summary>
	public class BundleMetadata : FileData<BundleFileEntry>
	{
		public BundleMetadata(Stream stream, string filePath, bool isClosable) :
			base(stream, isClosable)
		{
			if(string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(filePath);
			}
			m_filePath = filePath;
		}

		public BundleMetadata(Stream stream, string filePath, bool isClosable, IReadOnlyList<BundleFileEntry> entries):
			this(stream, filePath, isClosable)
		{
			m_entries = entries;
		}

		public void ReadPre530(EndianReader reader)
		{
			if (m_stream != reader.BaseStream)
			{
				throw new ArgumentException("Stream doesn't match", nameof(reader));
			}

			long basePosition = reader.BaseStream.Position;

			int count = reader.ReadInt32();
			BundleFileEntry[] entries = new BundleFileEntry[count];
			for (int i = 0; i < count; i++)
			{
				string name = reader.ReadStringZeroTerm();
				int offset = reader.ReadInt32();
				int size = reader.ReadInt32();

				long globalOffset = basePosition + offset;
				BundleFileEntry entry = new BundleFileEntry(m_stream, m_filePath, name, globalOffset, size, IsDisposable);
				entries[i] = entry;
			}
			m_entries = entries;
		}

		public void Read530(EndianReader reader, long basePosition)
		{
			int count = reader.ReadInt32();
			BundleFileEntry[] entries = new BundleFileEntry[count];
			for (int i = 0; i < count; i++)
			{
				long offset = reader.ReadInt64();
				long size = reader.ReadInt64();
				int blobIndex = reader.ReadInt32();
				string name = reader.ReadStringZeroTerm();
				
				long globalOffset = basePosition + offset;
				BundleFileEntry entry = new BundleFileEntry(m_stream, m_filePath, name, globalOffset, size, IsDisposable);
				entries[i] = entry;
			}
			m_entries = entries;
		}

		public override IReadOnlyList<BundleFileEntry> Entries => m_entries;

		private readonly string m_filePath;

		private IReadOnlyList<BundleFileEntry> m_entries;
	}
}
