using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Interfaces
{
	public interface IUnityObjectBase : IUnityAssetBase, IConvertToEditor
	{
		AssetInfo AssetInfo { get; set; }
		ClassIDType ClassID { get; }
		string ExportExtension { get; }
		string ExportPath { get; }
		ISerializedFile SerializedFile { get; }
		UnityGUID GUID { get; set; }
		long PathID { get; }
		HideFlags ObjectHideFlags { get; set; }

		IUnityObjectBase ConvertLegacy(IExportContainer container);
		YAMLDocument ExportYAMLDocument(IExportContainer container);
	}
}
