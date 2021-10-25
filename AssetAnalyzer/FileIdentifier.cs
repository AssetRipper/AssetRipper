using System;

namespace AssetAnalyzer
{
	public class FileIdentifier
	{
		public Guid guid;
		public int type; //enum { kNonAssetType = 0, kDeprecatedCachedAssetType = 1, kSerializedAssetType = 2, kMetaAssetType = 3 };
		public string pathName;

		//custom
		public string fileName;
	}
}
