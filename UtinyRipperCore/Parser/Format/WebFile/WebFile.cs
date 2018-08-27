using Brotli;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UtinyRipper.WebFiles
{
	internal class WebFile : IDisposable
	{
		public WebFile(FileCollection fileCollection, string filePath, Action<string> requestDependencyCallback)
		{
			if (fileCollection == null)
			{
				throw new ArgumentNullException(nameof(fileCollection));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			if (requestDependencyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestDependencyCallback));
			}

			m_fileCollection = fileCollection;
			m_filePath = filePath;
			m_requestDependencyCallback = requestDependencyCallback;
		}

		public static bool IsWebFile(string webPath)
		{
			if (!FileMultiStream.Exists(webPath))
			{
				throw new Exception($"Web at path '{webPath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(webPath))
			{
				return IsWebFile(stream);
			}
		}

		public static bool IsWebFile(Stream baseStream)
		{
			try
			{
				using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
				{
					long position = stream.BaseStream.Position;
					ulong magic = stream.ReadUInt16();
					if (magic == GZipMagic)
					{
						stream.BaseStream.Position = position;
						return true;
					}

					stream.BaseStream.Position = position + 0x20;
					magic = (stream.ReadUInt64() & 0xFFFFFFFFFFFF0000) >> 16;
					if (magic == BrotliMagic)
					{
						stream.BaseStream.Position = position;
						return true;
					}

					stream.BaseStream.Position = position;
					string signature = stream.ReadStringZeroTerm();
					if (signature == Signature)
					{
						stream.BaseStream.Position = position;
						return true;
					}

					stream.BaseStream.Position = position;
					return false;
				}
			}
			catch
			{
				return false;
			}
		}
		
		public void Load(string webPath)
		{
			if (!FileUtils.Exists(webPath))
			{
				throw new Exception($"WebFile at path '{webPath}' doesn't exist");
			}

			FileStream stream = FileUtils.OpenRead(webPath);
			Read(stream, true);
		}
		
		public void Read(Stream baseStream)
		{
			Read(baseStream, false);
		}

		public void Dispose()
		{
			Metadata.Dispose();
		}

		private void Read(Stream baseStream, bool isClosable)
		{
			ReadMetadata(baseStream, isClosable);

			foreach (WebFileEntry entry in Metadata.ResourceEntries)
			{
				entry.ReadResourcesFile(m_fileCollection);
			}
			foreach (WebFileEntry entry in Metadata.AssetsEntries)
			{
				entry.ReadFile(m_fileCollection, OnRequestDependency);
			}
		}

		private void ReadMetadata(Stream baseStream, bool isClosable)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long position = stream.BaseStream.Position;
				ulong magic = stream.ReadUInt16();
				if (magic == GZipMagic)
				{
					stream.BaseStream.Position = position;
					ReadGZip(stream, isClosable);
					return;
				}

				stream.BaseStream.Position = position + 0x20;
				magic = (stream.ReadUInt64() & 0xFFFFFFFFFFFF0000) >> 16;
				if (magic == BrotliMagic)
				{
					stream.BaseStream.Position = position;
					ReadBrotli(stream, isClosable);
					return;
				}

				stream.BaseStream.Position = position;
				stream.EndianType = EndianType.LittleEndian;
				ReadWebFiles(stream, isClosable);
			}
		}

		private void ReadGZip(EndianStream stream, bool isClosable)
		{
			MemoryStream memStream = new MemoryStream();
			using (GZipStream gzipStream = new GZipStream(stream.BaseStream, CompressionMode.Decompress))
			{
				gzipStream.CopyTo(memStream);
				memStream.Position = 0;
			}
			if (isClosable)
			{
				stream.Dispose();
			}
			using (EndianStream dataStream = new EndianStream(memStream, EndianType.LittleEndian))
			{
				ReadWebFiles(dataStream, true);
			}
		}

		private void ReadBrotli(EndianStream stream, bool isClosable)
		{
			MemoryStream memStream = new MemoryStream();
			using (BrotliInputStream brotliStream = new BrotliInputStream(stream.BaseStream))
			{
				brotliStream.CopyStream(memStream);
				memStream.Position = 0;
			}
			if (isClosable)
			{
				stream.Dispose();
			}
			using (EndianStream dataStream = new EndianStream(memStream, EndianType.LittleEndian))
			{
				ReadWebFiles(dataStream, true);
			}
		}

		private void ReadWebFiles(EndianStream stream, bool isClosable)
		{
			string signature = stream.ReadStringZeroTerm();
			if(signature != Signature)
			{
				throw new Exception($"Signature dosn't match to '{Signature}'");
			}

			List<WebFileEntry> entries = new List<WebFileEntry>();
			long headerLength = stream.ReadInt32();
			while(stream.BaseStream.Position < headerLength)
			{
				int offset = stream.ReadInt32();
				int length = stream.ReadInt32();
				int pathLength = stream.ReadInt32();
				string path = stream.ReadString(pathLength);
				
				WebFileEntry entry = new WebFileEntry(stream.BaseStream, m_filePath, path, offset, length, isClosable);
				entries.Add(entry);
			}
			Metadata = new WebMetadata(stream.BaseStream, isClosable, entries);
		}

		private void OnRequestDependency(string dependency)
		{
			if (FilenameUtils.ContainsDependency(m_loadedFiles, dependency))
			{
				return;
			}
			foreach (WebFileEntry entry in Metadata.AssetsEntries)
			{
				if (FilenameUtils.IsDependency(entry.Name, dependency))
				{
					m_loadedFiles.Add(entry.Name);
					entry.ReadFile(m_fileCollection, OnRequestDependency);
					return;
				}
			}

			m_requestDependencyCallback.Invoke(dependency);
		}

		public WebMetadata Metadata { get; private set; }

		private const ushort GZipMagic = 0x1F8B;
		private const ulong BrotliMagic = 0x62726F746C69;
		private const string Signature = "UnityWebData1.0";

		private readonly HashSet<string> m_loadedFiles = new HashSet<string>();

		private readonly FileCollection m_fileCollection;
		private readonly string m_filePath;
		private readonly Action<string> m_requestDependencyCallback;
	}
}
