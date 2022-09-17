using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Extensions;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.MultiFile;
using AssetRipper.IO.Files.Streams.Smart;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AssetRipper.IO.Files.WebFiles
{
	public sealed class WebFile : FileContainer
	{
		private const string Signature = "UnityWebData1.0";
		public WebFileEntry[] Entries { get; set; } = Array.Empty<WebFileEntry>();

		public static bool IsWebFile(string webPath)
		{
			using Stream stream = MultiFileStream.OpenRead(webPath);
			return IsWebFile(stream);
		}

		public static bool IsWebFile(Stream stream)
		{
			using EndianReader reader = new EndianReader(stream, EndianType.LittleEndian);
			return IsWebFile(reader);
		}

		public override void Read(SmartStream stream)
		{
			long basePosition = stream.Position;
			using (EndianReader reader = new EndianReader(stream, EndianType.LittleEndian))
			{
				string signature = reader.ReadStringZeroTerm();
				Debug.Assert(signature == Signature, $"Signature '{signature}' doesn't match to '{Signature}'");

				long headerLength = reader.ReadInt32(); //total size of the header including the signature and all the entries.
				List<WebFileEntry> entries = new();
				while (reader.BaseStream.Position - basePosition < headerLength)
				{
					WebFileEntry entry = new();
					entry.Read(reader);
					entries.Add(entry);
				}
				Entries = entries.ToArray();
			}

			foreach (WebFileEntry entry in Entries)
			{
				byte[] buffer = new byte[entry.Size];
				stream.Position = entry.Offset + basePosition;
				stream.ReadBuffer(buffer, 0, buffer.Length);
				ResourceFile file = new ResourceFile(SmartStream.CreateMemory(buffer, 0, buffer.Length, false), FilePath, entry.NameOrigin);
				AddResourceFile(file);
			}
		}

		public override void Write(Stream stream)
		{
			long basePosition = stream.Position;
			using (EndianWriter writer = new EndianWriter(stream, EndianType.LittleEndian))
			{
				writer.WriteStringZeroTerm(Signature);

				long entriesStartPosition = basePosition + 4;
				writer.BaseStream.Position = entriesStartPosition;
				foreach (WebFileEntry entry in Entries)
				{
					writer.WriteEndian(entry);
				}
				long endPosition = writer.BaseStream.Position;
				writer.BaseStream.Position = basePosition;
				writer.Write((int)(endPosition - basePosition));
				writer.BaseStream.Position = endPosition;
			}

			throw new NotImplementedException();
		}

		internal static bool IsWebFile(EndianReader reader)
		{
			if (reader.BaseStream.Length - reader.BaseStream.Position > Signature.Length)
			{
				long position = reader.BaseStream.Position;
				bool isRead = reader.ReadStringZeroTerm(Signature.Length + 1, out string? signature);
				reader.BaseStream.Position = position;
				if (isRead)
				{
					return signature == Signature;
				}
			}
			return false;
		}
	}
}
