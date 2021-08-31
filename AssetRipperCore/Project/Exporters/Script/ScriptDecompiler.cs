using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;
using System.IO;
using ICSharpCode.Decompiler;
using AssetRipper.Core.Structure.Assembly.Managers;

namespace AssetRipper.Core.Project.Exporters.Script
{
	public class ScriptDecompiler
	{
		private AssemblyResolver assemblyResolver;

		public ScriptDecompiler(IAssemblyManager assemblyManager) : this(assemblyManager.GetAssemblies()) { }
		public ScriptDecompiler(AssemblyDefinition assembly) : this(new AssemblyDefinition[] { assembly }) { }
		public ScriptDecompiler(AssemblyDefinition[] assemblies)
		{
			assemblyResolver = new AssemblyResolver(assemblies);
		}

		public string Decompile(TypeDefinition definition)
		{
			var decompiler = MakeDecompiler(definition.Module.Assembly);

			foreach (var module in decompiler.TypeSystem.Modules)
			{
				foreach (var type in module.TypeDefinitions)
				{
					if (definition.FullName == type.FullName)
					{
						return decompiler.DecompileTypeAsString(type.FullTypeName);
					}
				}
			}

			return null;
		}

		private CSharpDecompiler MakeDecompiler(AssemblyDefinition assembly)
		{
			DecompilerSettings settings = new DecompilerSettings();
			settings.LoadInMemory = true;
			return new CSharpDecompiler(assemblyResolver.Resolve(assembly.FullName), assemblyResolver, settings);
		}

		private static void RemoveAllMethods(AssemblyDefinition assembly)
		{
			foreach(var module in assembly.Modules)
			{
				foreach(var type in module.Types)
				{
					RemoveAllMethods(type);
				}
			}
		}

		private static void RemoveAllMethods(TypeDefinition typeDefinition)
		{
			typeDefinition.Methods.Clear();
			foreach (var nestedType in typeDefinition.NestedTypes)
				RemoveAllMethods(nestedType);
		}

		private static AssemblyDefinition DeepCloneAssembly(AssemblyDefinition original)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				original.Write(memoryStream);
				memoryStream.Position = 0;
				return AssemblyDefinition.ReadAssembly(memoryStream);
			}
		}
	}
}
