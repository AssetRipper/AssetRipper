using System.Collections.Generic;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper
{
	public interface IFileCollection
	{
		ISerializedFile FindSerializedFile(FileIdentifier identifier);
		ResourcesFile FindResourcesFile(ISerializedFile file, string fileName);

		T FindAsset<T>()
			where T: Object;
		T FindAsset<T>(string name)
			where T : NamedObject;
		IEnumerable<Object> FetchAssets();

		bool IsScene(ISerializedFile file);

		AssetFactory AssetFactory { get; }
	}
}
