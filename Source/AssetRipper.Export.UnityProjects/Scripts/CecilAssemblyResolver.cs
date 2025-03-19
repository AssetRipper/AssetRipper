using AsmResolver.DotNet;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing.Assemblies;
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
		/// Current solution:<br />
		///  * The types from these assemblies always seem to be included in mscorlib, we currently use a <see cref="ForwardingAssemblyGenerator"/>
		///    to enable these types to be resolved.
		/// </remarks>
		private readonly ConcurrentDictionary<string, PEFile> peAssemblies = new();
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

		public MetadataFile? Resolve(IAssemblyReference reference)
		{
			if (peAssemblies.TryGetValue(reference.Name, out PEFile? peResult))
			{
				return peResult;
			}
			else
			{
				Logger.Warning(LogCategory.Export, $"Could not resolve assembly: {reference.Name}");
				return null;
			}
		}

		public PEFile Resolve(AssemblyDefinition assembly) => peAssemblies[assembly.Name!];

		public Task<MetadataFile?> ResolveAsync(IAssemblyReference reference)
		{
			return Task.Run(() => Resolve(reference));
		}

		/// <summary>
		/// Finds a module in the same directory as another
		/// </summary>
		/// <param name="mainModule"></param>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public MetadataFile? ResolveModule(MetadataFile mainModule, string moduleName)
		{
			MetadataFile? result = peAssemblies.Values.Where(x => x.Name == moduleName).SingleOrDefault();
			if (result is not null)
			{
			}
			else
			{
				Logger.Warning(LogCategory.Export, $"Could not resolve module: {moduleName}");
			}
			return result;
		}

		public Task<MetadataFile?> ResolveModuleAsync(MetadataFile mainModule, string moduleName)
		{
			return Task.Run(() => ResolveModule(mainModule, moduleName));
		}
	}
}
