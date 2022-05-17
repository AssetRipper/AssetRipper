using AssetRipper.Core.Configuration;
using AssetRipper.Core.Structure.Assembly.Managers;
using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;
using System;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal class ScriptDecompiler
	{
		private readonly CecilAssemblyResolver assemblyResolver;
		public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp7_3;
		public ScriptContentLevel ScriptContentLevel { get; set; } = ScriptContentLevel.Level2;

		public ScriptDecompiler(IAssemblyManager assemblyManager) : this(new CecilAssemblyResolver(assemblyManager)) { }
		public ScriptDecompiler(AssemblyDefinition assembly) : this(new CecilAssemblyResolver(assembly)) { }
		public ScriptDecompiler(AssemblyDefinition[] assemblies) : this(new CecilAssemblyResolver(assemblies)) { }
		private ScriptDecompiler(CecilAssemblyResolver cecilAssemblyResolver) => assemblyResolver = cecilAssemblyResolver;

		public void DecompileWholeProject(AssemblyDefinition assembly, string outputFolder)
		{
			WholeAssemblyDecompiler decompiler = new WholeAssemblyDecompiler(assemblyResolver);
			decompiler.Settings.SetLanguageVersion(LanguageVersion);
			decompiler.Settings.UseNestedDirectoriesForNamespaces = true;
			if (ScriptContentLevel == ScriptContentLevel.Level1)
			{
				decompiler.CustomTransforms.Add(new MethodStripper());
			}
			DecompileWholeProject(decompiler, assembly, outputFolder);
		}

		private void DecompileWholeProject(WholeAssemblyDecompiler decompiler, AssemblyDefinition assembly, string outputFolder)
		{
			decompiler.DecompileProject(
				 assemblyResolver.Resolve(assembly) ?? throw new Exception($"Could not resolve {assembly.FullName}"),
				 outputFolder);
		}
	}
}
