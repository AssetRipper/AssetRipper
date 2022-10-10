using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.Yaml;

namespace AssetRipper.Assets;

public interface IUnityObjectBase : IUnityAssetBase
{
	AssetInfo AssetInfo { get; }
	int ClassID { get; }
	string ClassName { get; }
	AssetCollection Collection { get; }
	long PathID { get; }
	UnityGUID GUID { get; }
	string? OriginalPath { get; set; }
	string? OriginalDirectory { get; set; }
	string? OriginalName { get; set; }
	string? OriginalExtension { get; set; }
	string? AssetBundleName { get; set; }

	YamlDocument ExportYamlDocument(IExportContainer container);
}
