using AssetRipper.Core.Configuration;
using AssetRipper.Core.Structure.Assembly.Managers;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal class ScriptDecompiler
	{
		private readonly Dictionary<AssemblyDefinition, CSharpDecompiler> decompilers = new Dictionary<AssemblyDefinition, CSharpDecompiler>();
		private readonly CecilAssemblyResolver assemblyResolver;
		private LanguageVersion m_languageVersion = LanguageVersion.CSharp7_3;
		private ScriptContentLevel m_ScriptContentLevel = ScriptContentLevel.Level2;

		public ScriptDecompiler(IAssemblyManager assemblyManager) : this(new CecilAssemblyResolver(assemblyManager)) { }
		public ScriptDecompiler(AssemblyDefinition assembly) : this(new CecilAssemblyResolver(assembly)) { }
		public ScriptDecompiler(AssemblyDefinition[] assemblies) : this(new CecilAssemblyResolver(assemblies)) { }
		private ScriptDecompiler(CecilAssemblyResolver cecilAssemblyResolver) => assemblyResolver = cecilAssemblyResolver;

		public LanguageVersion LanguageVersion
		{
			get => m_languageVersion;
			set
			{
				if (value != m_languageVersion)
				{
					m_languageVersion = value;
					decompilers.Clear();
				}
			}
		}

		public ScriptContentLevel ScriptContentLevel
		{
			get => m_ScriptContentLevel;
			set
			{
				if (m_ScriptContentLevel != value)
				{
					m_ScriptContentLevel = value;
					decompilers.Clear();
				}
			}
		}

		public string Decompile(TypeDefinition definition)
		{
			var decompiler = GetOrMakeDecompiler(definition.Module.Assembly);
			return decompiler.DecompileTypeAsString(new FullTypeName(GetReflectionName(definition, decompiler)));
		}

		private string GetReflectionName(TypeDefinition definition, CSharpDecompiler decompiler)
		{
			if (!definition.IsNested && !definition.HasGenericParameters)
				return definition.FullName;
			foreach (IModule module in decompiler.TypeSystem.Modules)
			{
				foreach (ITypeDefinition type in module.TypeDefinitions)
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
			if (!decompilers.TryGetValue(assembly, out CSharpDecompiler result))
			{
				result = MakeDecompiler(assembly);
				decompilers.Add(assembly, result);
			}
			return result;
		}

		private CSharpDecompiler MakeDecompiler(AssemblyDefinition assembly)
		{
			DecompilerSettings settings = new DecompilerSettings();
			settings.SetLanguageVersion(m_languageVersion);
			settings.ShowXmlDocumentation = true;
			settings.LoadInMemory = true; //pulled from ILSpy code for reading a pe file from a stream
			CSharpDecompiler decompiler = new CSharpDecompiler(assemblyResolver.Resolve(assembly.FullName), assemblyResolver, settings);
			if (ScriptContentLevel == ScriptContentLevel.Level1)
			{
				decompiler.AstTransforms.Insert(0, new MethodStripper());
			}
			return decompiler;
		}
	}
}
