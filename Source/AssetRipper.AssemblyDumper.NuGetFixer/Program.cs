using ICSharpCode.SharpZipLib.Zip;
using System.Xml.Linq;

namespace AssetRipper.AssemblyDumper.NuGetFixer;

internal static class Program
{
	static void Main(string[] args)
	{
		string nupkgFolder = args[0];
		string version = args[1];
		string nupkgFileName = $"AssetRipper.SourceGenerated.{version}.nupkg";
		string nupkgFilePath = Path.Combine(nupkgFolder, nupkgFileName);

		string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirectory);

		string packageDirectory = Path.Combine(tempDirectory, "package");
		Directory.CreateDirectory(packageDirectory);
		ExtractZip(nupkgFilePath, packageDirectory);

		// Modify the .nuspec file
		{
			string nuspecFilePath = Directory.GetFiles(packageDirectory, "*.nuspec", SearchOption.TopDirectoryOnly).Single();
			RemoveDependencyReferences(nuspecFilePath);
		}

		// Repackage the .nupkg file
		{
			string newNupkgFilePath = Path.Combine(AppContext.BaseDirectory, nupkgFileName);
			if (File.Exists(newNupkgFilePath))
			{
				File.Delete(newNupkgFilePath);
			}

			CreateZip(newNupkgFilePath, packageDirectory);
		}

		// Clean up the temporary directory
		Directory.Delete(tempDirectory, true);

		Console.WriteLine("Dependency references removed successfully.");
	}

	static void RemoveDependencyReferences(string nuspecFilePath)
	{
		XDocument doc = XDocument.Load(nuspecFilePath);

		XElement dependenciesElement = doc.GetChild("package")
			.GetChild("metadata")
			.GetChild("dependencies");
		foreach (XElement framework in dependenciesElement.Elements())
		{
			framework.Elements().Remove();
		}

		doc.Save(nuspecFilePath);
	}

	private static void CreateZip(string zipFilePath, string sourceDirectory)
	{
		new FastZip().CreateZip(zipFilePath, sourceDirectory, true, null);
	}

	private static void ExtractZip(string zipFilePath, string targetDirectory)
	{
		new FastZip().ExtractZip(zipFilePath, targetDirectory, null);
	}

	private static XElement GetChild(this XContainer parent, string localName)
	{
		return parent.Elements().Single(x => x.Name.LocalName == localName);
	}
}
