using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core
{
	public interface IUnityObjectBase : IAssetNew
	{
		AssetInfo AssetInfo { get; set; }
		ClassIDType ClassID { get; }
		string ExportExtension { get; }
		string ExportPath { get; }
		ISerializedFile File { get; }
		UnityGUID GUID { get; }
		long PathID { get; }

		IUnityObjectBase Convert(IExportContainer container);
		YAMLDocument ExportYAMLDocument(IExportContainer container);
	}
}