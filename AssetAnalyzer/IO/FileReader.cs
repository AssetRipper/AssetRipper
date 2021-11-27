using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.IO.Extensions;
using System;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.IO.FileReading
{
	public class FileReader : EndianReader
	{
		public string FullPath { get; }
		public string FileName { get; }
		public FileType FileType { get; }

		private static byte[] gzipMagic = { 0x1f, 0x8b };
		private static byte[] brotliMagic = { 0x62, 0x72, 0x6F, 0x74, 0x6C, 0x69 };

		public FileReader(string path) : this(path, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

		public FileReader(string path, Stream stream) : base(stream, EndianType.BigEndian)
		{
			FullPath = Path.GetFullPath(path);
			FileName = Path.GetFileName(path);
			FileType = CheckFileType();
		}

		private FileType CheckFileType()
		{
			var signature = this.ReadStringToNull(20);
			BaseStream.Position = 0;
			switch (signature)
			{
				case "UnityWeb":
				case "UnityRaw":
				case "UnityArchive":
				case "UnityFS":
					return FileType.BundleFile;
				case "UnityWebData1.0":
					return FileType.WebFile;
				default:
					{
						var magic = ReadBytes(2);
						BaseStream.Position = 0;
						if (gzipMagic.SequenceEqual(magic))
						{
							return FileType.WebFile;
						}
						BaseStream.Position = 0x20;
						magic = ReadBytes(6);
						BaseStream.Position = 0;
						if (brotliMagic.SequenceEqual(magic))
						{
							return FileType.WebFile;
						}
						if (IsSerializedFile())
						{
							return FileType.AssetsFile;
						}
						else
						{
							return FileType.ResourceFile;
						}
					}
			}
		}

		private bool IsSerializedFile()
		{
			var fileSize = BaseStream.Length;
			if (fileSize < 20)
			{
				return false;
			}
			var m_MetadataSize = ReadUInt32();
			long m_FileSize = ReadUInt32();
			var m_Version = ReadUInt32();
			long m_DataOffset = ReadUInt32();
			var m_Endianess = ReadByte();
			var m_Reserved = ReadBytes(3);
			if (m_Version >= 22)
			{
				if (fileSize < 48)
				{
					BaseStream.Position = 0;
					return false;
				}
				m_MetadataSize = ReadUInt32();
				m_FileSize = ReadInt64();
				m_DataOffset = ReadInt64();
			}
			BaseStream.Position = 0;
			if (m_FileSize != fileSize)
			{
				return false;
			}
			if (m_DataOffset > fileSize)
			{
				return false;
			}
			return true;
		}
	}
}
