using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Reading;
using AssetRipper.Core.SerializedFiles;
using System;

namespace AssetRipper.Core.IO
{
	public class ObjectReader : EndianReader
	{
		public SerializedFile assetsFile;
		public long m_PathID;
		public long byteStart;
		public uint byteSize;
		public ClassIDType type;
		public SerializedType serializedType;
		public Platform platform;
		public SerializedFileFormatVersion m_Version;

		public int[] version => assetsFile.version;
		public BuildType buildType => assetsFile.buildType;

		public ObjectReader(EndianReader reader, SerializedFile assetsFile, ObjectInfo objectInfo) : base(reader.BaseStream, reader.EndianType)
		{
			this.assetsFile = assetsFile;
			m_PathID = objectInfo.m_PathID;
			byteStart = objectInfo.byteStart;
			byteSize = objectInfo.byteSize;
			if (Enum.IsDefined(typeof(ClassIDType), objectInfo.classID))
			{
				type = (ClassIDType)objectInfo.classID;
			}
			else
			{
				type = ClassIDType.UnknownType;
			}
			serializedType = objectInfo.serializedType;
			platform = assetsFile.m_TargetPlatform;
			m_Version = assetsFile.header.m_Version;
		}

		public void Reset()
		{
			Position = byteStart;
		}
	}
}
