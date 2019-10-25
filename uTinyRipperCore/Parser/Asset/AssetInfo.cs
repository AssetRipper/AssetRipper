using System;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper
{
	public class AssetInfo
	{
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID):
			this(serializedFile, pathID, classID, new GUID(Guid.NewGuid()))
		{
		}

		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, GUID guid)
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

		public GUID GUID;
	}
}
