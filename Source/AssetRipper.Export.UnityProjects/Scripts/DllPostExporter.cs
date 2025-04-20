﻿using AsmResolver.DotNet;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public class DllPostExporter : IPostExporter
	{
		public void DoPostExport(GameData gameData, LibraryConfiguration settings, FileSystem fileSystem)
		{
			Logger.Info(LogCategory.Export, "Saving game assemblies...");
			IAssemblyManager assemblyManager = gameData.AssemblyManager;
			AssemblyDefinition[] assemblies = assemblyManager.GetAssemblies().ToArray();
			if (assemblies.Length != 0)
			{
				string outputDirectory = fileSystem.Path.Join(settings.AuxiliaryFilesPath, "GameAssemblies");

				fileSystem.Directory.Create(outputDirectory);
				foreach (AssemblyDefinition assembly in assemblies)
				{
					string filepath = fileSystem.Path.Join(outputDirectory, SpecialFileNames.AddAssemblyFileExtension(assembly.Name!));
					using Stream stream = fileSystem.File.Create(filepath);
					assemblyManager.SaveAssembly(assembly, stream);
				}
			}
		}
	}
}
