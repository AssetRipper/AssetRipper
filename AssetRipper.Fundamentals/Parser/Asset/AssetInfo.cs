using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files.SerializedFiles;

namespace AssetRipper.Core.Parser.Asset
{
	public class AssetInfo
	{
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID) 
			: this(serializedFile, pathID, classID, UnityGUID.NewGuid()) { }
		public AssetInfo(ISerializedFile serializedFile, long pathID, ClassIDType classID, UnityGUID guid)
		{
			File = serializedFile ?? throw new ArgumentNullException(nameof(serializedFile));
			PathID = pathID;
			ClassID = classID;
			GUID = guid;
		}

		public ISerializedFile File { get; }
		public long PathID { get; }
		public ClassIDType ClassID { get; }
		public int ClassNumber => (int)ClassID;
		public UnityGUID GUID { get; set; }
	}
}
