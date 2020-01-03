using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Game;

namespace uTinyRipper
{
	public interface IFileCollection
	{
		ISerializedFile FindSerializedFile(string fileName);
		IResourceFile FindResourceFile(string fileName);

		T FindAsset<T>()
			where T: Object;
		T FindAsset<T>(string name)
			where T : NamedObject;
		IEnumerable<Object> FetchAssets();

		bool IsScene(ISerializedFile file);

		AssetFactory AssetFactory { get; }
		IAssemblyManager AssemblyManager { get; }
	}
}
