using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System;

namespace AssetRipper.Core.Parser.Asset
{
	public class AssetInfo
	{
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID) : this(serializedFile, pathID, classID, -1, UnityGUID.NewGuid()) { }
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, int byteSize) : this(serializedFile, pathID, classID, byteSize, UnityGUID.NewGuid()) { }
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, int byteSize, UnityGUID guid)
		{
			if (serializedFile == null)
			{
				throw new ArgumentNullException(nameof(serializedFile));
			}
			File = serializedFile;

			PathID = pathID;
			ClassID = classID;
			ClassNumber = (int)classID;
			ByteSize = byteSize;
			GUID = guid;
		}

		public ISerializedFile File { get; }

		public long PathID { get; }
		public ClassIDType ClassID { get; }
		public int ClassNumber { get; }
		public UnityGUID GUID { get; set; }
		public int ByteSize { get; }
	}
}
