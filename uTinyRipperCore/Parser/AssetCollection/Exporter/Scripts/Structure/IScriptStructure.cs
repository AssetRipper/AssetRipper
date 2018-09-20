using uTinyRipper.Classes;

namespace uTinyRipper.AssetExporters
{
	public interface IScriptStructure : IAssetReadable, IYAMLExportable, IDependent
	{
		IScriptStructure CreateCopy();
		
		IScriptStructure Base { get; }
		string Namespace { get; }
		string Name { get; }
	}
}
