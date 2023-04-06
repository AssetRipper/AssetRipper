using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
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
	IUnityObjectBase? MainAsset { get; set; }

	YamlDocument ExportYamlDocument(IExportContainer container);
	IEnumerable<(FieldName, PPtr<IUnityObjectBase>)> FetchDependencies();
}
public static class UnityObjectBaseExtensions
{
	public static void Delete(this IUnityObjectBase asset, bool throwIfReferenced) => asset.Collection.DeleteAsset(asset, throwIfReferenced);
	public static void Read(this IUnityObjectBase asset, ref EndianSpanReader reader) => asset.Read(ref reader, asset.Collection.Flags);
}
