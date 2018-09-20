using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.WebFiles
{
	public sealed class WebFile : IDisposable
	{
		public WebFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			m_filePath = filePath;
		}

		~WebFile()
		{
			Dispose(false);
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

		public static bool IsWebFile(Stream stream)
		{
			using (EndianReader reader = new EndianReader(stream, stream.Position, EndianType.BigEndian))
			{
				long position = reader.BaseStream.Position;
				long maxLengthLong = reader.BaseStream.Length - position;
				int maxLength = maxLengthLong > int.MaxValue ? int.MaxValue : (int)maxLengthLong;
				if (reader.ReadStringZeroTerm(maxLength, out string signature))
				{
					if (signature == Signature)
					{
						reader.BaseStream.Position = position;
						return true;
					}
				}

				reader.BaseStream.Position = position;
				return false;
			}
		}

		public static WebFile Load(string webPath)
		{
			WebFile web = new WebFile(webPath);
			web.Load();
			return web;
		}

		public static WebFile Read(SmartStream stream, string webPath)
		{
			WebFile web = new WebFile(webPath);
			web.Read(stream);
			return web;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if(Metadata != null)
			{
				Metadata.Dispose();
			}
		}

		private void Load()
		{
			if (!FileUtils.Exists(m_filePath))
			{
				throw new Exception($"WebFile at path '{m_filePath}' doesn't exist");
			}

			using (SmartStream stream = SmartStream.OpenRead(m_filePath))
			{
				Read(stream);
			}
		}

		private void Read(SmartStream stream)
		{
			using (EndianReader reader = new EndianReader(stream, stream.Position, EndianType.LittleEndian))
			{
				ReadMetadata(reader);
			}
		}

		private void ReadMetadata(EndianReader reader)
		{
			string signature = reader.ReadStringZeroTerm();
			if(signature != Signature)
			{
				throw new Exception($"Signature '{signature}' doesn't match to '{Signature}'");
			}

			SmartStream webStream = (SmartStream)reader.BaseStream;
			List<WebFileEntry> entries = new List<WebFileEntry>();
			long headerLength = reader.ReadInt32();
			while(webStream.Position < headerLength)
			{
				int offset = reader.ReadInt32();
				int length = reader.ReadInt32();
				int pathLength = reader.ReadInt32();
				string path = reader.ReadString(pathLength);
				
				WebFileEntry entry = new WebFileEntry(webStream, m_filePath, path, offset, length);
				entries.Add(entry);
			}
			Metadata = new WebMetadata(entries);
		}

		public WebMetadata Metadata { get; private set; }

		private const string Signature = "UnityWebData1.0";

		private readonly string m_filePath;
	}
}
