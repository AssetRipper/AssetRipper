using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.Assembly.Managers;
using ICSharpCode.Decompiler.Metadata;
using Mono.Cecil;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal class CecilAssemblyResolver : ICSharpCode.Decompiler.Metadata.IAssemblyResolver
	{
		private readonly ConcurrentDictionary<string, PEFile> peAssemblies = new();
		public CecilAssemblyResolver(IAssemblyManager manager) : this(manager.GetAssemblies()) { }
		public CecilAssemblyResolver(ReadOnlySpan<AssemblyDefinition> assemblies)
		{
			foreach (AssemblyDefinition assembly in assemblies)
			{
				PEFile peFile = CreatePEFile(assembly);
				if (!peAssemblies.TryAdd(assembly.Name.Name, peFile))
				{
					throw new Exception($"Could not add pe assembly: {assembly.Name.Name} to name dictionary!");
				}
			}
		}
		public CecilAssemblyResolver(AssemblyDefinition loneAssembly)
		{
			PEFile peFile = CreatePEFile(loneAssembly);
			if (!peAssemblies.TryAdd(loneAssembly.Name.Name, peFile))
			{
				throw new Exception($"Could not add pe assembly: {loneAssembly.Name.Name} to name dictionary!");
			}
		}

		private static PEFile CreatePEFile(AssemblyDefinition assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			MemoryStream memoryStream = new MemoryStream();
			assembly.Write(memoryStream);
			memoryStream.Position = 0;

			return new PEFile(assembly.Name.Name, memoryStream);
		}

		public PEFile? Resolve(IAssemblyReference reference) => ResolveFromName(reference.Name);

		public PEFile? Resolve(AssemblyDefinition assembly) => ResolveFromName(assembly.Name.Name);

		public PEFile? Resolve(string fullName)
		{
			int index = fullName.IndexOf(", Version=", StringComparison.Ordinal);
			return ResolveFromName(fullName.Substring(0, index));
		}

		/// <remarks>
		/// Todo: implicit references<br />
		/// In <see cref="ICSharpCode.Decompiler.TypeSystem.DecompilerTypeSystem"/>, it states:<br /><br />
		///   For .NET Core and .NET 5 and newer, we need to pull in implicit references which are not included in the metadata,<br />
		///   as they contain compile-time-only types, such as System.Runtime.InteropServices.dll (for DllImport, MarshalAs, etc.)<br /><br />
		/// As a result, it tries to load these assemblies:<br />
		///  * System.Runtime.InteropServices<br />
		///  * System.Runtime.CompilerServices.Unsafe<br /><br />
		/// Possible solutions:<br />
		///	 * The types from these assemblies seem to be included in mscorlib, so forwarding assemblies might work.<br />
		///	 * Including the ones from a .NET installation as stored resources.
		/// </remarks>
		private PEFile? ResolveFromName(string name)
		{
			if (peAssemblies.TryGetValue(name, out PEFile? result))
			{
				return result;
			}
			else
			{
				Logger.Warning(LogCategory.Export, $"Could not resolve assembly: {name}");
				return null;
			}
		}

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
			if (result is null)
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
