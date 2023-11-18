using AsmResolver.DotNet;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files.Utils;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public class DllPostExporter : IPostExporter
	{
		public void DoPostExport(GameData gameData, LibraryConfiguration settings)
		{
			string outputDirectory = Path.Combine(settings.AuxiliaryFilesPath, "GameAssemblies");

			Logger.Info(LogCategory.Export, "Saving game assemblies...");
			IAssemblyManager assemblyManager = gameData.AssemblyManager;
			AssemblyDefinition[] assemblies = assemblyManager.GetAssemblies().ToArray();
			if (assemblies.Length != 0)
			{
				Directory.CreateDirectory(outputDirectory);
				foreach (AssemblyDefinition assembly in assemblies)
				{
					string filepath = Path.Combine(outputDirectory, FilenameUtils.AddAssemblyFileExtension(assembly.Name!));
					assemblyManager.SaveAssembly(assembly, filepath);
				}
			}
		}
	}
}
