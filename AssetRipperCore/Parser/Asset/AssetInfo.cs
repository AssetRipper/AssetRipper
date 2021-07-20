using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.Files.SerializedFile;
using System;

namespace AssetRipper.Parser.Asset
{
	public class AssetInfo
	{
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID) :
			this(serializedFile, pathID, classID, new UnityGUID(Guid.NewGuid()))
		{
		}

		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, UnityGUID guid)
		{
			if (serializedFile == null)
			{
				throw new ArgumentNullException(nameof(serializedFile));
			}
			File = serializedFile;

			PathID = pathID;
			ClassID = classID;
			GUID = guid;
		}

		public ISerializedFile File { get; }

		public long PathID { get; }
		public ClassIDType ClassID { get; }

		public UnityGUID GUID;
	}
}
