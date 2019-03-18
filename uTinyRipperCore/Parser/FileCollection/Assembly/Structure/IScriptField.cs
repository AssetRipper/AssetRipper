using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

namespace uTinyRipper.Assembly
{
	public interface IScriptField : IAssetReadable, IYAMLExportable, IDependent
	{
		IScriptField CreateCopy();
	}
}
