using UtinyRipper.AssetExporters;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public interface IPPtr<T> : IAssetReadable, IYAMLExportable
		where T: Object
	{
		T FindAsset(ISerializedFile file);
		T GetAsset(ISerializedFile file);

		bool IsNull { get; }
	}
}
