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
			EntriesBase = entries;
		}

		public void ReadPre530(EndianStream stream)
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
				BundleFileEntry entry = new BundleFileEntry(m_stream, m_filePath, name, globalOffset, size, IsDisposable);
				entries[i] = entry;
			}
			EntriesBase = entries;
		}

		public void Read530(EndianStream stream, long basePosition)
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
				BundleFileEntry entry = new BundleFileEntry(m_stream, m_filePath, name, globalOffset, size, IsDisposable);
				entries[i] = entry;
			}
			EntriesBase = entries;
		}

		public IReadOnlyList<BundleFileEntry> Entries => EntriesBase;

		private readonly string m_filePath;
	}
}
