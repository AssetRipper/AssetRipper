using AssemblyDumper.Unity;
using System.IO;
using System.Text.Json;

namespace AssemblyDumper.Passes
{
	public static class Pass000_Initialize
	{
		/// <summary>
		/// Used to determine which version of the AssemblyDumper generated an assembly
		/// </summary>
		private static Version currentGeneratedVersion = new Version(0, 0, 0, 0);

		/// <summary>
		/// Read the information json, system assembly, and AssetRipperCommon. Then create a new assembly.
		/// </summary>
		public static void DoPass(string jsonPath, string systemRuntimeFilePath, string systemCollectionsFilePath)
		{
			Console.WriteLine("Pass 000: Initialize");

			using FileStream stream = File.OpenRead(jsonPath);
			UnityInfo info = JsonSerializer.Deserialize<UnityInfo>(stream)!;
			SharedState.Initialize(info);

			string AssemblyFileName = '_' + SharedState.Version.Replace('.', '_');

			AssemblyDefinition assembly = new AssemblyDefinition(AssemblyFileName, currentGeneratedVersion);
			ModuleDefinition module = new ModuleDefinition(AssemblyFileName, KnownCorLibs.SystemRuntime_v6_0_0_0);
			assembly.Modules.Add(module);

			AssemblyDefinition runtimeAssembly = AssemblyDefinition.FromFile(systemRuntimeFilePath);
			AssemblyDefinition collectionsAssembly = AssemblyDefinition.FromFile(systemCollectionsFilePath);
			AssemblyDefinition commonAssembly = AssemblyDefinition.FromFile(typeof(AssetRipper.Core.UnityObjectBase).Assembly.Location);

			SystemTypeGetter.RuntimeAssembly = runtimeAssembly;
			SystemTypeGetter.CollectionsAssembly = collectionsAssembly;
			CommonTypeGetter.CommonAssembly = commonAssembly;

			module.MetadataResolver.AssemblyResolver.AddToCache(runtimeAssembly, runtimeAssembly);
			module.MetadataResolver.AssemblyResolver.AddToCache(collectionsAssembly, collectionsAssembly);
			module.MetadataResolver.AssemblyResolver.AddToCache(commonAssembly, commonAssembly);

			SharedState.Assembly = assembly;
			SharedState.RootNamespace = AssemblyFileName;
			SharedState.Importer = new ReferenceImporter(module);

			CommonTypeGetter.Initialize();
			SystemTypeGetter.Initialize(assembly.ManifestModule!);
		}
	}
}