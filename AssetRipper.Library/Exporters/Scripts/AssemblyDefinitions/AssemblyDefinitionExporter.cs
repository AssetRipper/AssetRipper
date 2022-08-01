using Mono.Cecil;
using System.IO;
using System.Text.Json;

namespace AssetRipper.Library.Exporters.Scripts.AssemblyDefinitions
{
	public static class AssemblyDefinitionExporter
	{
		public static void Export(AssemblyDefinitionDetails details)
		{
			string assetPath = Path.Combine(details.OutputFolder, $"{details.AssemblyName}.asmdef");

			AssemblyDefinitionAsset asset = new AssemblyDefinitionAsset(details.AssemblyName);
			if (details.Assembly is not null)
			{
				foreach (AssemblyNameReference reference in details.Assembly.MainModule.AssemblyReferences)
				{
					if (ReferenceAssemblies.IsReferenceAssembly(reference.Name))
					{
						continue;
					}

					asset.References.Add(reference.Name);
				}
			}

			string assetData = JsonSerializer.Serialize(asset, AssemblyDefinitionSerializerContext.Default.AssemblyDefinitionAsset);
			File.WriteAllText(assetPath, assetData);
		}
	}
}
