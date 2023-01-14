using AsmResolver.DotNet;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using ICSharpCode.Decompiler.Metadata;
using System.Collections.Concurrent;
using IAssemblyResolver = ICSharpCode.Decompiler.Metadata.IAssemblyResolver;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	internal class CecilAssemblyResolver : IAssemblyResolver
	{
		/// <remarks>
		/// In <see cref="ICSharpCode.Decompiler.TypeSystem.DecompilerTypeSystem"/>, it states:<br /><br />
		///   For .NET Core and .NET 5 and newer, we need to pull in implicit references which are not included in the metadata,<br />
		///   as they contain compile-time-only types, such as System.Runtime.InteropServices.dll (for DllImport, MarshalAs, etc.)<br /><br />
		/// As a result, it tries to load these assemblies:<br />
		///  * System.Runtime.InteropServices<br />
		///  * System.Runtime.CompilerServices.Unsafe<br /><br />
		/// Possible solutions:<br />
		///	 * The types from these assemblies seem to be included in mscorlib, so forwarding assemblies might work.<br />
		///	 * Including the ones from a .NET installation as stored resources.<br />
		///	Current solution:<br />
		///	 * We currently use a <see cref="backupResolver"/> to resolve references from local .NET installations.
		///	   Given that this just causes a failed resolve on System.Private.CoreLib, it might not be doing much.
		/// </remarks>
		private readonly ConcurrentDictionary<string, PEFile> peAssemblies = new();
		private readonly UniversalAssemblyResolver backupResolver = new UniversalAssemblyResolver(null, false, null);
		public CecilAssemblyResolver(IAssemblyManager manager)
		{
			foreach (AssemblyDefinition assembly in manager.GetAssemblies())
			{
				Stream stream = manager.GetStreamForAssembly(assembly);
				stream.Position = 0;
				PEFile peFile = new PEFile(assembly.Name!, stream);
				if (!peAssemblies.TryAdd(assembly.Name!, peFile))
				{
					throw new Exception($"Could not add pe assembly: {assembly.Name} to name dictionary!");
				}
			}
		}

		public PEFile? Resolve(IAssemblyReference reference)
		{
			if (peAssemblies.TryGetValue(reference.Name, out PEFile? result))
			{
				return result;
			}
			else if (backupResolver.TryResolve(reference, out result))
			{
				Logger.Info(LogCategory.Export, $"Assembly resolved from local .NET installation: {reference.Name}");
				return result;
			}
			else
			{
				Logger.Warning(LogCategory.Export, $"Could not resolve assembly: {reference.Name}");
				return null;
			}
		}

		public PEFile Resolve(AssemblyDefinition assembly) => peAssemblies[assembly.Name!];

		public Task<PEFile?> ResolveAsync(IAssemblyReference reference)
		{
			return Task.Run(() => Resolve(reference));
		}

		/// <summary>
		/// Finds a module in the same directory as another
		/// </summary>
		/// <param name="mainModule"></param>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public PEFile? ResolveModule(PEFile mainModule, string moduleName)
		{
			PEFile? result = peAssemblies.Values.Where(x => x.Name == moduleName).SingleOrDefault();
			if (result is not null)
			{
			}
			else if (backupResolver.TryResolveModule(mainModule, moduleName, out result))
			{
				Logger.Info(LogCategory.Export, $"Module resolved from local .NET installation: {moduleName}");
			}
			else
			{
				Logger.Warning(LogCategory.Export, $"Could not resolve module: {moduleName}");
			}
			return result;
		}

		public Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
		{
			return Task.Run(() => ResolveModule(mainModule, moduleName));
		}
	}
}
