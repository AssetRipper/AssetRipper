using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public interface IScriptField : IAssetReadable, IYAMLExportable, IDependent
	{
		IScriptField CreateCopy();
	}
}
