using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AssetRipper.Import.Structure.Assembly.Managers;

public partial class BaseManager
{
	private sealed class Resolver : IAssemblyResolver
	{
		private static readonly SignatureComparer Comparer = new(SignatureComparisonFlags.VersionAgnostic);
		private readonly Dictionary<AssemblyDescriptor, AssemblyDefinition> cache = new(Comparer);
		private readonly BaseManager assemblyManager;

		public Resolver(BaseManager assemblyManager)
		{
			this.assemblyManager = assemblyManager;
		}

		public void AddToCache(AssemblyDescriptor descriptor, AssemblyDefinition definition)
		{
			if (cache.ContainsKey(descriptor))
			{
				throw new ArgumentException("The cache already contains an entry of assembly " + descriptor.FullName + ".", "descriptor");
			}
			if (!Comparer.Equals(descriptor, definition))
			{
				throw new ArgumentException("Assembly descriptor and definition do not refer to the same assembly.");
			}
			cache.Add(descriptor, definition);
		}

		public void ClearCache()
		{
			cache.Clear();
		}

		public bool HasCached(AssemblyDescriptor descriptor)
		{
			return cache.ContainsKey(descriptor);
		}

		public bool RemoveFromCache(AssemblyDescriptor descriptor)
		{
			return cache.Remove(descriptor);
		}

		public AssemblyDefinition? Resolve(AssemblyDescriptor assembly)
		{
			if (cache.TryGetValue(assembly, out AssemblyDefinition? value))
			{
				return value;
			}
			value = ResolveImplementation(assembly);
			if (value != null)
			{
				cache.Add(assembly, value);
			}
			return value;
		}

		private AssemblyDefinition? ResolveImplementation(AssemblyDescriptor assembly)
		{
			string? name = assembly.Name;
			if (name is not null && assemblyManager.m_assemblies.TryGetValue(name, out AssemblyDefinition? assemblyDefinition))
			{
				return assemblyDefinition;
			}
			else
			{
				return null;
			}
		}
	}

}
