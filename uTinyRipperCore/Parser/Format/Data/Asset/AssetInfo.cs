using System;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	public class AssetInfo
	{
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID):
			this(serializedFile, pathID, classID, new EngineGUID(Guid.NewGuid()))
		{
		}

		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, EngineGUID guid)
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

		public EngineGUID GUID;
	}
}
