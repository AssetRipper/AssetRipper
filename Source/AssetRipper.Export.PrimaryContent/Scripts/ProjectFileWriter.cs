using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;
using System.Xml;

namespace AssetRipper.Export.PrimaryContent.Scripts;

internal sealed class ProjectFileWriter : ProjectFileWriterSdkStyle
{
	const string TrueString = "True";
	const string FalseString = "False";

	public static ProjectFileWriter Instance { get; } = new();

	protected override string GetTargetFrameworkMoniker(MetadataFile module, IProjectInfoProvider project)
	{
		// We need to define a target framework for the project, even though the assembly won't actually target the reference assemblies for that framework.
		return "net10.0";
	}

	protected override IEnumerable<AssemblyReference> GetReferences(MetadataFile module, IProjectInfoProvider project)
	{
		return module.AssemblyReferences;
	}

	protected override void WriteReference(XmlTextWriter xml, AssemblyReference reference, IProjectInfoProvider project)
	{
		xml.WriteStartElement("ProjectReference");
		xml.WriteAttributeString("Include", $"../{reference.Name}/{reference.Name}.csproj");
		xml.WriteEndElement();
	}

	protected override IEnumerable<(string, string)> GetCustomProperties(IProjectInfoProvider project, IEnumerable<ProjectItemInfo> files, MetadataFile module)
	{
		// We need to define a target framework for the project, even though the assembly won't actually target the reference assemblies for that framework.
		yield return ("TargetFramework", "net10.0");

		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/advanced#nostandardlib
		yield return ("NoStandardLib", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#appendtargetframeworktooutputpath
		yield return ("AppendTargetFrameworkToOutputPath", FalseString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#appendruntimeidentifiertooutputpath
		yield return ("AppendRuntimeIdentifierToOutputPath", FalseString);

		// https://github.com/dotnet/runtime/blob/72dac24a0e6fa1047002858a762f36c88e53850b/src/coreclr/System.Private.CoreLib/System.Private.CoreLib.csproj#L51
		yield return ("DisableImplicitConfigurationDefines", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disableimplicitframeworkdefines
		yield return ("DisableImplicitFrameworkDefines", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disableimplicitframeworkreferences
		yield return ("DisableImplicitFrameworkReferences", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disabletransitiveprojectreferences
		yield return ("DisableTransitiveProjectReferences", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disableimplicitnamespaceimports
		yield return ("DisableImplicitNamespaceImports", TrueString);

		// https://github.com/dotnet/runtime/blob/72dac24a0e6fa1047002858a762f36c88e53850b/src/coreclr/System.Private.CoreLib/System.Private.CoreLib.csproj#L8
		yield return ("EnsureRuntimePackageDependencies", FalseString);

		// https://github.com/dotnet/runtime/blob/72dac24a0e6fa1047002858a762f36c88e53850b/src/coreclr/System.Private.CoreLib/System.Private.CoreLib.csproj#L40-L41
		yield return ("AddAdditionalExplicitAssemblyReferences", FalseString);

		// https://github.com/dotnet/runtime/blob/72dac24a0e6fa1047002858a762f36c88e53850b/src/coreclr/System.Private.CoreLib/System.Private.CoreLib.csproj#L42
		if (module.Name == "mscorlib")
		{
			yield return ("RuntimeMetadataVersion", "v4.0.30319");
		}
	}
}
