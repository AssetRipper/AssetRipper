using AssetRipper.Core.Logging;
using Mono.Cecil;
using System.IO;
using System.Text.Json;

namespace AssetRipper.Library.Exporters.Scripts.AssemblyDefinitions
{
	public static class AssemblyDefinitionExporter
	{
		public static void Export(AssemblyDefinition assembly, string outputFolder)
		{
			string assetPath = Path.Combine(outputFolder, assembly.Name.Name + ".asmdef");
			Logger.Info(LogCategory.Export, $"Exporting assembly definition '{assembly.Name.Name}.asmdef'");

			AssemblyDefinitionAsset asset = new(assembly.Name.Name);
			foreach (AssemblyNameReference reference in assembly.MainModule.AssemblyReferences)
			{
				if (ReferenceAssemblies.IsReferenceAssembly(reference.Name))
				{
					continue;
				}

				asset.references.Add(reference.Name);
			}

			string assetData = JsonSerializer.Serialize(asset, new JsonSerializerOptions()
			{
				IncludeFields = true,
				WriteIndented = true,
			});
			File.WriteAllText(assetPath, assetData);
		}
	}
}
