using AsmResolver.DotNet;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Managers;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using System.IO;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal class ScriptDecompiler
	{
		private readonly CecilAssemblyResolver assemblyResolver;
		public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp7_3;
		public ScriptContentLevel ScriptContentLevel { get; set; } = ScriptContentLevel.Level2;
		public ScriptingBackend ScriptingBackend { get; set; } = ScriptingBackend.Unknown;

		public ScriptDecompiler(IAssemblyManager assemblyManager) : this(new CecilAssemblyResolver(assemblyManager), assemblyManager.ScriptingBackend) { }
		private ScriptDecompiler(CecilAssemblyResolver cecilAssemblyResolver, ScriptingBackend scriptingBackend)
		{
			assemblyResolver = cecilAssemblyResolver;
			ScriptingBackend = scriptingBackend;
		}

		public void DecompileWholeProject(AssemblyDefinition assembly, string outputFolder)
		{
			WholeProjectDecompiler decompiler = new(assemblyResolver);

			decompiler.Settings.AlwaysShowEnumMemberValues = true;
			decompiler.Settings.ShowXmlDocumentation = true;

			decompiler.Settings.SetLanguageVersion(LanguageVersion);
			decompiler.Settings.UseNestedDirectoriesForNamespaces = true;

			DecompileWholeProject(decompiler, assembly, outputFolder);
		}

		private void DecompileWholeProject(WholeProjectDecompiler decompiler, AssemblyDefinition assembly, string outputFolder)
		{
			decompiler.DecompileProject(assemblyResolver.Resolve(assembly), outputFolder, new StringWriter());
		}
	}
}
