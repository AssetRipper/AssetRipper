using AssetRipper.Core.Structure.Assembly;
using System.Collections.Generic;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	public interface IPlatformGameStructure
	{
		Dictionary<string, string> Assemblies { get; }
		ScriptingBackend Backend { get; }
		IReadOnlyList<string> DataPaths { get; }
		Dictionary<string, string> Files { get; }
		string GameDataPath { get; }
		string Il2CppGameAssemblyPath { get; }
		string Il2CppMetaDataPath { get; }
		string ManagedPath { get; }
		string Name { get; }
		string ResourcesPath { get; }
		string RootPath { get; }
		string StreamingAssetsPath { get; }
		string UnityPlayerPath { get; }
		int[] UnityVersion { get; }

		void CollectFiles(bool skipStreamingAssets);
		string RequestAssembly(string assembly);
		string RequestDependency(string dependency);
		string RequestResource(string resource);
	}
}