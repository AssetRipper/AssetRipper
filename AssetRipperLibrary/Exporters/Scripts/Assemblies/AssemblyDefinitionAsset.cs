using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts.Assemblies
{
	public class AssemblyDefinitionAsset
	{
		public string name;
		public List<string> references = new();
		public bool allowUnsafeCode;

		public AssemblyDefinitionAsset(string name)
		{
			this.name = name;
			allowUnsafeCode = true;
		}
	}
}
