using System;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper
{
	public class AssetInfo
	{
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID):
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
		public ClassIDType ClassID  { get; }

		public UnityGUID GUID;
	}
}
