using uTinyRipper.Classes;

namespace uTinyRipper.AssetExporters
{
	public interface IScriptField : IAssetReadable, IYAMLExportable, IDependent
	{
		IScriptField CreateCopy();
	}
}
