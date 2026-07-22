using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;
using System.Xml;

namespace AssetRipper.Export.PrimaryContent.Scripts;

internal sealed class ProjectFileWriter : IProjectFileWriter
{
	const string TrueString = "True";
	const string FalseString = "False";

	public static ProjectFileWriter Instance { get; } = new();

	public void Write(TextWriter target, IProjectInfoProvider project, IEnumerable<ProjectItemInfo> files, MetadataFile module)
	{
		using XmlTextWriter xmlWriter = new(target);
		xmlWriter.Formatting = Formatting.Indented;
		Write(xmlWriter, project, files, module);
	}

	private static void Write(XmlTextWriter xml, IProjectInfoProvider project, IEnumerable<ProjectItemInfo> files, MetadataFile module)
	{
		xml.WriteStartElement("Project");

		xml.WriteAttributeString("Sdk", "Microsoft.NET.Sdk");

		PlaceIntoTag("PropertyGroup", xml, () => WriteAssemblyInfo(xml, module));
		PlaceIntoTag("PropertyGroup", xml, () => WriteFrameworkInfo(xml));
		PlaceIntoTag("PropertyGroup", xml, () => WriteProjectInfo(xml, project));
		PlaceIntoTag("ItemGroup", xml, () => WriteResources(xml, files));
		PlaceIntoTag("ItemGroup", xml, () => WriteReferences(xml, module));

		xml.WriteEndElement();
	}

	static void PlaceIntoTag(string tagName, XmlTextWriter xml, Action content)
	{
		xml.WriteStartElement(tagName);
		try
		{
			content();
		}
		finally
		{
			xml.WriteEndElement();
		}
	}

	static void WriteAssemblyInfo(XmlTextWriter xml, MetadataFile module)
	{
		xml.WriteElementString("AssemblyName", module.Name);

		// Since we create AssemblyInfo.cs manually, we need to disable the auto-generation
		xml.WriteElementString("GenerateAssemblyInfo", FalseString);
	}

	static void WriteFrameworkInfo(XmlTextWriter xml)
	{
		// We need to define a target framework for the project, even though the assembly won't actually target the reference assemblies for that framework.
		xml.WriteElementString("TargetFramework", "net10.0");

		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/advanced#nostandardlib
		xml.WriteElementString("NoStandardLib", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#appendtargetframeworktooutputpath
		xml.WriteElementString("AppendTargetFrameworkToOutputPath", FalseString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#appendruntimeidentifiertooutputpath
		xml.WriteElementString("AppendRuntimeIdentifierToOutputPath", FalseString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disableimplicitframeworkdefines
		xml.WriteElementString("DisableImplicitFrameworkDefines", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disableimplicitframeworkreferences
		xml.WriteElementString("DisableImplicitFrameworkReferences", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disabletransitiveprojectreferences
		xml.WriteElementString("DisableTransitiveProjectReferences", TrueString);

		// https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disableimplicitnamespaceimports
		xml.WriteElementString("DisableImplicitNamespaceImports", TrueString);
	}

	static void WriteProjectInfo(XmlTextWriter xml, IProjectInfoProvider project)
	{
		xml.WriteElementString("LangVersion", project.LanguageVersion.ToString().Replace("CSharp", "").Replace('_', '.'));
		xml.WriteElementString("AllowUnsafeBlocks", TrueString);
		xml.WriteElementString("CheckForOverflowUnderflow", project.CheckForOverflowUnderflow ? TrueString : FalseString);

		if (project.StrongNameKeyFile != null)
		{
			xml.WriteElementString("SignAssembly", TrueString);
			xml.WriteElementString("AssemblyOriginatorKeyFile", Path.GetFileName(project.StrongNameKeyFile));
		}
	}

	static void WriteResources(XmlTextWriter xml, IEnumerable<ProjectItemInfo> files)
	{
		// remove phase
		foreach (ProjectItemInfo item in files.Where(t => t.ItemType == "EmbeddedResource"))
		{
			string buildAction = Path.GetExtension(item.FileName).ToUpperInvariant() switch
			{
				".CS" => "Compile",
				".RESX" => "EmbeddedResource",
				_ => "None"
			};
			if (buildAction == "EmbeddedResource")
			{
				continue;
			}

			xml.WriteStartElement(buildAction);
			xml.WriteAttributeString("Remove", item.FileName);
			xml.WriteEndElement();
		}

		// include phase
		foreach (ProjectItemInfo item in files.Where(t => t.ItemType == "EmbeddedResource"))
		{
			if (Path.GetExtension(item.FileName) == ".resx")
			{
				continue;
			}

			xml.WriteStartElement("EmbeddedResource");
			xml.WriteAttributeString("Include", item.FileName);
			if (item.AdditionalProperties != null)
			{
				foreach ((string key, string value) in item.AdditionalProperties)
				{
					xml.WriteAttributeString(key, value);
				}
			}
			xml.WriteEndElement();
		}
	}

	static void WriteReferences(XmlTextWriter xml, MetadataFile module)
	{
		foreach (AssemblyReference reference in module.AssemblyReferences)
		{
			xml.WriteStartElement("ProjectReference");
			xml.WriteAttributeString("Include", $"../{reference.Name}/{reference.Name}.csproj");
			xml.WriteEndElement();
		}
	}
}
