using AssetRipper.Core.Logging;
using Mono.Cecil;
using System.IO;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class DllPostExporter : IPostExporter
	{
		public void DoPostExport(Ripper ripper)
		{
			string outputDirectory = Path.Combine(ripper.Settings.AuxiliaryFilesPath, "GameAssemblies");

			Logger.Info(LogCategory.Export, "Saving game assemblies...");
			AssemblyDefinition[] assemblies = ripper.GameStructure.FileCollection.AssemblyManager.GetAssemblies();
			if (assemblies.Length != 0)
			{
				Directory.CreateDirectory(outputDirectory);
				foreach (AssemblyDefinition? assembly in assemblies)
				{
					string filepath = Path.Combine(outputDirectory, assembly.Name.Name);
					if (!filepath.EndsWith(".dll"))
					{
						filepath += ".dll";
					}

					assembly.Write(filepath);
				}
			}
		}
	}
}
