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

			CustomWholeProjectDecompiler decompiler = new(settings, assemblyResolver, fileSystem);

			DecompileWholeProject(decompiler, assembly, outputFolder);
		}

		private void DecompileWholeProject(WholeProjectDecompiler decompiler, AssemblyDefinition assembly, string outputFolder)
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

		private sealed class CustomWholeProjectDecompiler(DecompilerSettings settings, CecilAssemblyResolver assemblyResolver, FileSystem fileSystem) : WholeProjectDecompiler(settings, assemblyResolver, null, null, null)
		{
			protected override void CreateDirectory(string path)
			{
				try
				{
					fileSystem.Directory.Create(path);
				}
				catch (IOException)
				{
					fileSystem.File.Delete(path);
					fileSystem.Directory.Create(path);
				}
			}

			protected override TextWriter CreateFile(string path)
			{
				Stream stream = fileSystem.File.Create(path);
				return new StreamWriter(stream);
			}
		}
	}
}
