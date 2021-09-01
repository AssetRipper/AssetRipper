using AssetRipper.Core.Structure.Assembly.Managers;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;

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

		//TODO optimize
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
			settings.SetLanguageVersion(LanguageVersion.CSharp7_3);
			settings.ShowXmlDocumentation = true;
			settings.LoadInMemory = true; //pulled from ILSpy code for reading a pe file from a stream
			return new CSharpDecompiler(assemblyResolver.Resolve(assembly.FullName), assemblyResolver, settings);
		}
	}
}
