using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Interfaces
{
	public interface IUnityObjectBase : IUnityAssetBase, IConvertToEditor
	{
		AssetInfo AssetInfo { get; set; }
		string AssetClassName { get; }
		ClassIDType ClassID { get; }
		string ExportExtension { get; }
		string ExportPath { get; }
		ISerializedFile SerializedFile { get; }
		UnityGUID GUID { get; set; }
		long PathID { get; }
		HideFlags ObjectHideFlagsOld { get; set; }

		IUnityObjectBase ConvertLegacy(IExportContainer container);
		YamlDocument ExportYamlDocument(IExportContainer container);
	}
}
