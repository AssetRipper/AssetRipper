using AssetRipper.Core.Configuration;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Library.Exporters.Scripts.Transform;
using AssetRipper.Library.Exporters.Scripts.Transforms;
using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;

namespace AssetRipper.Library.Exporters.Scripts
{
	internal class ScriptDecompiler
	{
		private readonly CecilAssemblyResolver assemblyResolver;
		public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp7_3;
		public ScriptContentLevel ScriptContentLevel { get; set; } = ScriptContentLevel.Level2;
		public ScriptingBackend ScriptingBackend { get; set; } = ScriptingBackend.Unknown;

		public ScriptDecompiler(IAssemblyManager assemblyManager) : this(new CecilAssemblyResolver(assemblyManager), assemblyManager.ScriptingBackend) { }
		public ScriptDecompiler(AssemblyDefinition assembly, ScriptingBackend scriptingBackend) : this(new CecilAssemblyResolver(assembly), scriptingBackend) { }
		public ScriptDecompiler(AssemblyDefinition[] assemblies, ScriptingBackend scriptingBackend) : this(new CecilAssemblyResolver(assemblies), scriptingBackend) { }
		private ScriptDecompiler(CecilAssemblyResolver cecilAssemblyResolver, ScriptingBackend scriptingBackend)
		{
			assemblyResolver = cecilAssemblyResolver;
			ScriptingBackend = scriptingBackend;
		}

		public void DecompileWholeProject(AssemblyDefinition assembly, string outputFolder)
		{
			WholeAssemblyDecompiler decompiler = new(assemblyResolver);
			// these settings may need to be changed later because
			// CSharpDecompiler.IsMemberHidden seems to contradict
			// what these settings state they do.
			decompiler.Settings.AnonymousTypes = false;
			decompiler.Settings.AnonymousMethods = false;
			decompiler.Settings.AsyncEnumerator = false;

			decompiler.Settings.AlwaysShowEnumMemberValues = true;
			decompiler.Settings.ShowXmlDocumentation = true;

			decompiler.Settings.SetLanguageVersion(LanguageVersion);
			decompiler.Settings.UseNestedDirectoriesForNamespaces = true;

			if (ScriptContentLevel == ScriptContentLevel.Level1)
			{
				decompiler.CustomTransforms.Add(new MemberStubTransform());
			}

			// code quality
			if (ScriptingBackend == ScriptingBackend.IL2Cpp && ScriptContentLevel <= ScriptContentLevel.Level2)
			{
				decompiler.CustomTransforms.Add(new RemoveInvalidMemberTransform(ScriptingBackend == ScriptingBackend.IL2Cpp));
				decompiler.CustomTransforms.Add(new FixOptionalParametersTransform());
				decompiler.CustomTransforms.Add(new ValidateNullCastsTransform());
				decompiler.CustomTransforms.Add(new FixExplicitInterfaceImplementationTransform());
				decompiler.CustomTransforms.Add(new FixStructLayoutAmbiguityTransform());
				decompiler.CustomTransforms.Add(new RemoveCompilerAttributeTransform());
				decompiler.CustomTransforms.Add(new FixGenericStructConstraintTransform());
			}

			// il2cpp fixes
			if (ScriptContentLevel == ScriptContentLevel.Level1 || // level one stubs everything, so it needs to be fixed up.
				(ScriptingBackend == ScriptingBackend.IL2Cpp && ScriptContentLevel <= ScriptContentLevel.Level2))
			{
				// maybe could be moved to code quality?
				decompiler.CustomTransforms.Add(new FixCompilerGeneratedAccessorsTransform());
				decompiler.CustomTransforms.Add(new EnsureOutParamsSetTransform());
				decompiler.CustomTransforms.Add(new EnsureStructFieldsSetTransform());
				decompiler.CustomTransforms.Add(new EnsureValidBaseConstructorTransform());
				decompiler.CustomTransforms.Add(new FixEventDeclarationsTransform());
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
