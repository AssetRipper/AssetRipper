using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
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

		T FindAsset<T>() where T : IUnityObjectBase;
		T FindAsset<T>(string name) where T : IUnityObjectBase, INamedObject;
		IEnumerable<IUnityObjectBase> FetchAssets();

		IEnumerable<IUnityObjectBase> FetchAssetsOfType(ClassIDType type);
		IEnumerable<IUnityObjectBase> FetchAssetsOfType<T>() where T : IUnityObjectBase;

		bool IsScene(ISerializedFile file);

		IAssemblyManager AssemblyManager { get; }
	}
}
