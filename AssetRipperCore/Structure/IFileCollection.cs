using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files.ResourceFiles;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Assembly;
using System.Collections.Generic;

namespace AssetRipper.Structure
{
	public interface IFileCollection
	{
		ISerializedFile FindSerializedFile(string fileName);
		IResourceFile FindResourceFile(string fileName);

		T FindAsset<T>()
			where T : Object;
		T FindAsset<T>(string name)
			where T : NamedObject;
		IEnumerable<Object> FetchAssets();

		bool IsScene(ISerializedFile file);

		AssetFactory AssetFactory { get; }
		IAssemblyManager AssemblyManager { get; }
	}
}
