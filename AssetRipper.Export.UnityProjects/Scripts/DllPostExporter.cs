using AsmResolver.DotNet;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public class DllPostExporter : IPostExporter
	{
		public void DoPostExport(Ripper ripper)
		{
			string outputDirectory = Path.Combine(ripper.Settings.AuxiliaryFilesPath, "GameAssemblies");

			Logger.Info(LogCategory.Export, "Saving game assemblies...");
			IAssemblyManager assemblyManager = ripper.GameStructure.AssemblyManager;
			AssemblyDefinition[] assemblies = assemblyManager.GetAssemblies().ToArray();
			if (assemblies.Length != 0)
			{
				Directory.CreateDirectory(outputDirectory);
				foreach (AssemblyDefinition assembly in assemblies)
				{
					string filepath = Path.Combine(outputDirectory, assembly.Name!);
					if (!filepath.EndsWith(".dll"))
					{
						filepath += ".dll";
					}

					Stream readStream = assemblyManager.GetStreamForAssembly(assembly);
					using FileStream writeStream = File.Create(filepath);
					readStream.Position = 0;
					readStream.CopyTo(writeStream);
				}
			}
		}
	}
}
