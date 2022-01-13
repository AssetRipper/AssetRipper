using AssetRipper.Core.Structure.Assembly.Managers;
using ICSharpCode.Decompiler.Metadata;
using Mono.Cecil;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal class CecilAssemblyResolver : ICSharpCode.Decompiler.Metadata.IAssemblyResolver
	{
		private readonly ConcurrentDictionary<string, PEFile> peAssemblies = new ConcurrentDictionary<string, PEFile>();
		public CecilAssemblyResolver(IAssemblyManager manager) : this(manager.GetAssemblies()) { }
		public CecilAssemblyResolver(ReadOnlySpan<AssemblyDefinition> assemblies)
		{
			foreach (var assembly in assemblies)
			{
				PEFile peFile = CreatePEFile(assembly);
				if (!peAssemblies.TryAdd(assembly.FullName, peFile))
				{
					throw new Exception($"Could not add pe assembly: {assembly.FullName} to name dictionary!");
				}
			}
		}
		public CecilAssemblyResolver(AssemblyDefinition loneAssembly)
		{
			PEFile peFile = CreatePEFile(loneAssembly);
			if (!peAssemblies.TryAdd(loneAssembly.FullName, peFile))
			{
				throw new Exception($"Could not add pe assembly: {loneAssembly.FullName} to name dictionary!");
			}
		}

		private static PEFile CreatePEFile(AssemblyDefinition assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			MemoryStream memoryStream = new MemoryStream();
			assembly.Write(memoryStream);
			memoryStream.Position = 0;

			return new PEFile(assembly.Name.Name, memoryStream);
		}

		public PEFile Resolve(IAssemblyReference reference) => Resolve(reference.FullName);

		public PEFile Resolve(string fullName)
		{
			if (peAssemblies.TryGetValue(fullName, out PEFile result))
				return result;
			else
				return null;
		}

		public Task<PEFile> ResolveAsync(IAssemblyReference reference)
		{
			return Task.Run(() => Resolve(reference));
		}

		/// <summary>
		/// Finds a module in the same directory as another
		/// </summary>
		/// <param name="mainModule"></param>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public PEFile ResolveModule(PEFile mainModule, string moduleName)
		{
			return peAssemblies.Values.Where(x => x.Name == moduleName).Single();
		}

		public Task<PEFile> ResolveModuleAsync(PEFile mainModule, string moduleName)
		{
			return Task.Run(() => ResolveModule(mainModule, moduleName));
		}
	}
}
