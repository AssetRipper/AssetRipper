using AssetRipper.Core.Classes;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Structure.Assembly.Managers;
using System.Collections.Generic;

namespace AssetRipper.Core.Structure
{
	public interface IFileCollection
	{
		ISerializedFile FindSerializedFile(string fileName);
		IResourceFile FindResourceFile(string fileName);

		T FindAsset<T>() where T : UnityObjectBase;
		T FindAsset<T>(string name) where T : NamedObject;
		IEnumerable<UnityObjectBase> FetchAssets();

		IEnumerable<UnityObjectBase> FetchAssetsOfType(ClassIDType type);

		bool IsScene(ISerializedFile file);

		AssetFactory AssetFactory { get; }
		IAssemblyManager AssemblyManager { get; }
	}
}
