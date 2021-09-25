using AssetRipper.Core.Structure.Assembly.Managers;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts
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

		private Dictionary<AssemblyDefinition, CSharpDecompiler> decompilers = new Dictionary<AssemblyDefinition, CSharpDecompiler>();
		
		public string Decompile(TypeDefinition definition)
		{
			var decompiler = GetOrMakeDecompiler(definition.Module.Assembly);
			return decompiler.DecompileTypeAsString(new FullTypeName(GetReflectionName(definition, decompiler)));
		}

		private string GetReflectionName(TypeDefinition definition, CSharpDecompiler decompiler)
		{
			if (!definition.IsNested && !definition.HasGenericParameters)
				return definition.FullName;
			foreach (var module in decompiler.TypeSystem.Modules)
			{
				foreach (var type in module.TypeDefinitions)
				{
					if (definition.FullName == type.FullName)
					{
						return type.FullTypeName.ReflectionName;
					}
				}
			}
			return definition.FullName;
		}

		private CSharpDecompiler GetOrMakeDecompiler(AssemblyDefinition assembly)
		{
			CSharpDecompiler result;
			if(!decompilers.TryGetValue(assembly, out result))
			{
				result = MakeDecompiler(assembly);
				decompilers.Add(assembly, result);
			}
			return result;
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
