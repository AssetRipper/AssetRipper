using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Meta.Importers.Asset
{
	public interface IAssetImporter : IUnityObjectBase
	{
		string AssetBundleName { get; set; }
		string AssetBundleVariant { get; set; }
		Dictionary<long, string> FileIDToRecycleName { get; set; }
		Dictionary<Tuple<ClassIDType, long>, string> InternalIDToNameTable { get; set; }
		long[] UsedFileIDs { get; set; }
		string UserData { get; set; }

		bool IncludesImporter(UnityVersion version);
	}
}