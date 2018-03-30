using UtinyRipper.AssetExporters;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public interface IPPtr<T> : IAssetReadable, IYAMLExportable
		where T: Object
	{
		T FindObject(ISerializedFile file);
		T GetObject(ISerializedFile file);

		bool IsNull { get; }
	}
}
