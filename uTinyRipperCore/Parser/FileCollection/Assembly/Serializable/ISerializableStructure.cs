using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;

namespace uTinyRipper.Assembly
{
	public interface ISerializableStructure : IAssetReadable, IYAMLExportable, IDependent
	{
		ISerializableStructure CreateDuplicate();
		//int CalculateSize(int depth);
	}
}
