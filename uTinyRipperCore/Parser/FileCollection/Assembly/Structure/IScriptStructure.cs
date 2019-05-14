using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

namespace uTinyRipper.Assembly
{
	public interface IScriptStructure : IAssetReadable, IYAMLExportable, IDependent
	{
		IScriptStructure CreateDuplicate();
		//int CalculateSize(int depth);
	}
}
