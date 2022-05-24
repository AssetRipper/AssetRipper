using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts.Assemblies
{
	public class AssemblyDefinitionAsset
	{
		public string name;
		public List<string> references = new();
		public List<string> includePlatforms = new();
		public List<string> excludePlatforms = new();
		public bool allowUnsafeCode;
		public bool overrideReferences;
		public List<string> precompiledReferences = new();
		public bool autoReferenced;
		public List<string> defineConstraints = new();
		public List<string> versionDefines = new();
		public bool noEngineReferences;

		public AssemblyDefinitionAsset(string name)
		{
			this.name = name;
		}
	}
}
