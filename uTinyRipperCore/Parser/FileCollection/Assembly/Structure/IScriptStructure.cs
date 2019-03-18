using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

namespace uTinyRipper.Assembly
{
	public interface IScriptStructure : IAssetReadable, IYAMLExportable, IDependent
	{
		IScriptStructure CreateCopy();
		
		IScriptStructure Base { get; }
		string Namespace { get; }
		string Name { get; }
	}
}
