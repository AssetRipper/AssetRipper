using AssetRipper.Parser.Asset;
using AssetRipper.Classes;
using AssetRipper.Classes.Object;
using AssetRipper.Parser.Files.ResourceFiles;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Assembly.Managers;
using System.Collections.Generic;

namespace AssetRipper.Structure
{
	public interface IFileCollection
	{
		ISerializedFile FindSerializedFile(string fileName);
		IResourceFile FindResourceFile(string fileName);

		T FindAsset<T>() where T : UnityObject;
		T FindAsset<T>(string name) where T : NamedObject;
		IEnumerable<UnityObject> FetchAssets();

		bool IsScene(ISerializedFile file);

		AssetFactory AssetFactory { get; }
		IAssemblyManager AssemblyManager { get; }
	}
}
