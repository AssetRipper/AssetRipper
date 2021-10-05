using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System;

namespace AssetRipper.Core.Parser.Asset
{
	public class AssetInfo
	{
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID) : this(serializedFile, pathID, classID, -1, new UnityGUID(Guid.NewGuid())) { }
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, int byteSize) : this(serializedFile, pathID, classID, byteSize, new UnityGUID(Guid.NewGuid())) { }
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, int byteSize, UnityGUID guid)
		{
			if (serializedFile == null)
			{
				throw new ArgumentNullException(nameof(serializedFile));
			}
			File = serializedFile;

			PathID = pathID;
			ClassID = classID;
			ByteSize = byteSize;
			GUID = guid;
		}

		public ISerializedFile File { get; }

		public long PathID { get; }
		public ClassIDType ClassID { get; }
		public UnityGUID GUID;
		public int ByteSize { get; }
	}
}
