using AsmResolver.DotNet;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing.Assemblies;
using ICSharpCode.Decompiler.Metadata;
using System.Collections.Concurrent;
using IAssemblyResolver = ICSharpCode.Decompiler.Metadata.IAssemblyResolver;

namespace AssetRipper.Export.UnityProjects.Scripts;

internal class CecilAssemblyResolver(IAssemblyManager manager) : IAssemblyResolver
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

	public MetadataFile? Resolve(IAssemblyReference reference)
	{
		return ResolveAssembly(reference.Name);
	}

	public PEFile Resolve(AssemblyDefinition assembly)
	{
		return ResolveAssembly(assembly.Name!)!;
	}

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
		return ResolveModule(moduleName);
	}

	public Task<MetadataFile?> ResolveModuleAsync(MetadataFile mainModule, string moduleName)
	{
		return Task.Run(() => ResolveModule(mainModule, moduleName));
	}

	private PEFile? ResolveAssembly(string referenceName)
	{
		if (peAssemblies.TryGetValue(referenceName, out PEFile? peResult))
		{
			return peResult;
		}
		else
		{
			lock (peAssemblies)
			{
				if (peAssemblies.TryGetValue(referenceName, out peResult))
				{
					return peResult;
				}

				AssemblyDefinition? assembly = manager.GetAssemblies().FirstOrDefault(x => x.Name == referenceName);
				if (assembly is not null)
				{
					Stream stream = manager.GetStreamForAssembly(assembly);
					stream.Position = 0;
					string assemblyName = assembly.Name!;
					peResult = new PEFile(assemblyName, stream);
					if (!peAssemblies.TryAdd(assemblyName, peResult))
					{
						throw new Exception($"Could not add pe assembly: {assemblyName} to name dictionary!");
					}
					return peResult;
				}
			}

			Logger.Warning(LogCategory.Export, $"Could not resolve assembly: {referenceName}");
			return null;
		}
	}

	private PEFile? ResolveModule(string moduleName)
	{
		lock (peAssemblies)
		{
			PEFile? result = peAssemblies.Values.Where(x => x.Name == moduleName).SingleOrDefault();
			if (result is not null)
			{
			}
			else
			{
				foreach (AssemblyDefinition assembly in manager.GetAssemblies())
				{
					if (!assembly.Modules.Any(m => m.Name == moduleName))
					{
						continue;
					}

					Stream stream = manager.GetStreamForAssembly(assembly);
					stream.Position = 0;
					string assemblyName = assembly.Name!;
					result = new PEFile(assemblyName, stream);
					if (!peAssemblies.TryAdd(assemblyName, result))
					{
						throw new Exception($"Could not add pe assembly: {assemblyName} to name dictionary!");
					}
					return result;
				}
				Logger.Warning(LogCategory.Export, $"Could not resolve module: {moduleName}");
			}
			return result;
		}
	}
}
