using Brotli;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using uTinyRipper.Assembly;
using uTinyRipper.SerializedFiles;
using uTinyRipper.WebFiles;

namespace uTinyRipper.ArchiveFiles
{
	public sealed class ArchiveFileScheme : FileScheme
	{
		private ArchiveFileScheme(SmartStream stream, long offset, long size, string filePath, string fileName) :
			base(stream, offset, size, filePath, fileName)
		{
		}

		internal static ArchiveFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			ArchiveFileScheme scheme = new ArchiveFileScheme(stream, offset, size, filePath, fileName);
			scheme.ReadScheme();
			scheme.ProcessEntry();
			return scheme;
		}

		public ArchiveFile ReadFile(IFileCollection collection, IAssemblyManager manager)
		{
			ArchiveFile archive = new ArchiveFile(collection, this);
			archive.AddFile(WebScheme, collection, manager);
			return archive;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			m_dataStream.Dispose();
		}

		private void ReadScheme()
		{
			using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
			{
				using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
				{
					Header.Read(reader);

					switch (Header.Type)
					{
						case ArchiveType.GZip:
							m_dataStream = ReadGZip(reader);
							break;

						case ArchiveType.Brotli:
							m_dataStream = ReadBrotli(reader);
							break;

						default:
							throw new NotSupportedException(Header.Type.ToString());
					}
				}
			}
		}

		private void ProcessEntry()
		{
			string name = Path.GetFileNameWithoutExtension(FilePath);
			WebScheme = WebFile.ReadScheme(m_dataStream, 0, m_stream.Length, FilePath, name);
		}

		private SmartStream ReadGZip(EndianReader reader)
		{
			using (SmartStream stream = SmartStream.CreateMemory())
			{
				using (GZipStream gzipStream = new GZipStream(reader.BaseStream, CompressionMode.Decompress))
				{
					gzipStream.CopyTo(stream);
				}
				return stream.CreateReference();
			}
		}

		private SmartStream ReadBrotli(EndianReader reader)
		{
			using (SmartStream stream = SmartStream.CreateMemory())
			{
				using (BrotliInputStream brotliStream = new BrotliInputStream(reader.BaseStream))
				{
					brotliStream.CopyTo(stream);
				}
				return stream.CreateReference();
			}
		}

		public override bool ContainsFile(string fileName)
		{
			return WebScheme.ContainsFile(fileName);
		}

		public ArchiveHeader Header { get; } = new ArchiveHeader();

		public override FileEntryType SchemeType => FileEntryType.Archive;
		public override IEnumerable<FileIdentifier> Dependencies => WebScheme.Dependencies;

		public WebFileScheme WebScheme { get; private set; }

		private SmartStream m_dataStream;
	}
}
