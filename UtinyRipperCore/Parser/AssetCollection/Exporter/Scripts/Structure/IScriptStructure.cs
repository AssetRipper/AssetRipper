using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public interface IScriptStructure : IAssetReadable, IYAMLExportable, IDependent
	{
		IScriptStructure CreateCopy();
		
		IScriptStructure Base { get; }
		string Namespace { get; }
		string Name { get; }
	}
}
