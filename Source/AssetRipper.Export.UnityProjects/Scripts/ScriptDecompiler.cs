using AsmResolver.DotNet;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	internal class ScriptDecompiler
	{
		private readonly CecilAssemblyResolver assemblyResolver;
		public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp7_3;
		public ScriptContentLevel ScriptContentLevel { get; set; } = ScriptContentLevel.Level2;
		public ScriptingBackend ScriptingBackend { get; set; } = ScriptingBackend.Unknown;
		public bool FullyQualifiedTypeNames { get; set; } = false;

		public ScriptDecompiler(IAssemblyManager assemblyManager) : this(new CecilAssemblyResolver(assemblyManager), assemblyManager.ScriptingBackend) { }
		private ScriptDecompiler(CecilAssemblyResolver cecilAssemblyResolver, ScriptingBackend scriptingBackend)
		{
			assemblyResolver = cecilAssemblyResolver;
			ScriptingBackend = scriptingBackend;
		}

		public void DecompileWholeProject(AssemblyDefinition assembly, string outputFolder, FileSystem fileSystem)
		{
			DecompilerSettings settings = new();

			settings.SetLanguageVersion(LanguageVersion);

			settings.AlwaysShowEnumMemberValues = true;
			settings.ShowXmlDocumentation = true;

			settings.UseSdkStyleProjectFormat = false;//sdk style can throw and we don't use the csproj file at all
			settings.UseNestedDirectoriesForNamespaces = true;

			if (FullyQualifiedTypeNames)
			{
				settings.AlwaysUseGlobal = true;
				settings.UsingDeclarations = false;
			}

			WholeProjectDecompiler decompiler = new(settings, assemblyResolver, null, null, null);

			DecompileWholeProject(decompiler, assembly, outputFolder, fileSystem);
		}

		private void DecompileWholeProject(WholeProjectDecompiler decompiler, AssemblyDefinition assembly, string outputFolder, FileSystem fileSystem)
		{
			try
			{
				decompiler.DecompileProject(assemblyResolver.Resolve(assembly), outputFolder, new StringWriter());
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}
		}
	}
}
