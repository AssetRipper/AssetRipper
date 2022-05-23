using AssetRipper.Core.Configuration;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Library.Exporters.Scripts.Transform;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using System;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal class ScriptDecompiler
	{
		private readonly CecilAssemblyResolver assemblyResolver;
		public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp7_3;
		public ScriptContentLevel ScriptContentLevel { get; set; } = ScriptContentLevel.Level2;
		public CodeCleanupHandler CodeCleanupHandler { get; set; }

		public ScriptDecompiler(IAssemblyManager assemblyManager, CodeCleanupSettings? cleanupSettings = null) : this(new CecilAssemblyResolver(assemblyManager), cleanupSettings) { }
		public ScriptDecompiler(AssemblyDefinition assembly, CodeCleanupSettings? cleanupSettings = null) : this(new CecilAssemblyResolver(assembly), cleanupSettings) { }
		public ScriptDecompiler(AssemblyDefinition[] assemblies, CodeCleanupSettings? cleanupSettings = null) : this(new CecilAssemblyResolver(assemblies), cleanupSettings) { }
		private ScriptDecompiler(CecilAssemblyResolver cecilAssemblyResolver, CodeCleanupSettings? cleanupSettings = null)
		{
            assemblyResolver = cecilAssemblyResolver;
			CodeCleanupHandler = new(cleanupSettings);
		}

		public CodeCleanupSettings CodeCleanupSettings
		{
			get => CodeCleanupHandler.Settings;
		}

		public void DecompileWholeProject(AssemblyDefinition assembly, string outputFolder)
		{
			WholeAssemblyDecompiler decompiler = new WholeAssemblyDecompiler(assemblyResolver);
			decompiler.Settings.SetLanguageVersion(LanguageVersion);
			decompiler.Settings.UseNestedDirectoriesForNamespaces = true;
			// these settings may need to be changed later because
			// CSharpDecompiler.IsMemberHidden seems to contradict
			// what these settings state they do.
			decompiler.Settings.AnonymousTypes = false;
			decompiler.Settings.AnonymousMethods = false;
			decompiler.Settings.AsyncEnumerator = false;

			decompiler.Settings.AlwaysShowEnumMemberValues = true;
			decompiler.Settings.ShowXmlDocumentation = true;
			if (ScriptContentLevel == ScriptContentLevel.Level1)
			{
				decompiler.CustomTransforms.Add(new MethodStripper());
			}
			CodeCleanupHandler.SetupDecompiler(decompiler);
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
