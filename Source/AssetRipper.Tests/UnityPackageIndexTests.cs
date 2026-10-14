using AssetRipper.Export.UnityProjects.Scripts.UnityPackages;
using AssetRipper.Primitives;

namespace AssetRipper.Tests;

internal class UnityPackageIndexTests
{
	private const string RuntimeScriptGuid = "0123456789abcdef0123456789abcdef";
	private const string ReferencedScriptGuid = "fedcba9876543210fedcba9876543210";
	private const string SampleScriptGuid = "00112233445566778899aabbccddeeff";
	private const string AssemblyDefinitionGuid = "aaaabbbbccccddddeeeeffff00001111";

	private string editorDirectory = "";

	[SetUp]
	public void CreateFakeEditor()
	{
		editorDirectory = Path.Join(Path.GetTempPath(), $"AssetRipperPackageIndex_{Guid.NewGuid():N}");
		string package = Path.Join(editorDirectory, "Editor", "Data", "Resources", "PackageManager", "BuiltInPackages", "com.example.package");

		Write(package, "package.json", """{ "name": "com.example.package", "version": "1.2.3" }""");
		Write(package, "Runtime/Example.Runtime.asmdef", """{ "name": "Example.Runtime", }""");
		Write(package, "Runtime/Example.Runtime.asmdef.meta", $"fileFormatVersion: 2\nguid: {AssemblyDefinitionGuid}\n");
		Write(package, "Runtime/Components/ExampleBehaviour.cs", "using UnityEngine;\nnamespace Example.Components\n{\n\tpublic class ExampleBehaviour : MonoBehaviour { }\n}\n");
		Write(package, "Runtime/Components/ExampleBehaviour.cs.meta", $"fileFormatVersion: 2\nguid: {RuntimeScriptGuid}\n");

		// A folder that belongs to the runtime assembly through an assembly definition reference
		Write(package, "Extra/Example.Extra.asmref", $$"""{ "reference": "GUID:{{AssemblyDefinitionGuid}}" }""");
		Write(package, "Extra/GlobalBehaviour.cs", "public class GlobalBehaviour : UnityEngine.MonoBehaviour { }\n");
		Write(package, "Extra/GlobalBehaviour.cs.meta", $"fileFormatVersion: 2\nguid: {ReferencedScriptGuid}\n");

		// Unity ignores folders ending with a tilde
		Write(package, "Samples~/Example.Samples.asmdef", """{ "name": "Example.Samples" }""");
		Write(package, "Samples~/SampleBehaviour.cs", "public class SampleBehaviour : UnityEngine.MonoBehaviour { }\n");
		Write(package, "Samples~/SampleBehaviour.cs.meta", $"fileFormatVersion: 2\nguid: {SampleScriptGuid}\n");

		static void Write(string root, string relativePath, string text)
		{
			string path = Path.Join(root, relativePath);
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.WriteAllText(path, text);
		}
	}

	[TearDown]
	public void DeleteFakeEditor()
	{
		Directory.Delete(editorDirectory, true);
	}

	[Test]
	public void ScriptsAreMappedToTheirPackageAssembly()
	{
		UnityPackageIndex index = UnityPackageIndex.Load(editorDirectory, new UnityVersion(6000, 3));
		Assert.That(index.TryGetAssembly("Example.Runtime", out PackageAssembly? assembly), Is.True);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(assembly!.PackageName, Is.EqualTo("com.example.package"));
			Assert.That(assembly.PackageVersion, Is.EqualTo("1.2.3"));
			Assert.That(assembly.TryGetScriptGuid("Example.Components", "ExampleBehaviour", out UnityGuid guid), Is.True);
			Assert.That(guid, Is.EqualTo(UnityGuid.Parse(RuntimeScriptGuid)));
		}
	}

	[Test]
	public void AssemblyDefinitionReferencesAddScriptsToTheReferencedAssembly()
	{
		UnityPackageIndex index = UnityPackageIndex.Load(editorDirectory, new UnityVersion(6000, 3));
		Assert.That(index.TryGetAssembly("Example.Runtime", out PackageAssembly? assembly), Is.True);
		Assert.That(assembly!.TryGetScriptGuid("", "GlobalBehaviour", out UnityGuid guid), Is.True);
		Assert.That(guid, Is.EqualTo(UnityGuid.Parse(ReferencedScriptGuid)));
	}

	[Test]
	public void ScriptsInAMovedNamespaceAreFoundByClassName()
	{
		UnityPackageIndex index = UnityPackageIndex.Load(editorDirectory, new UnityVersion(6000, 3));
		Assert.That(index.TryGetAssembly("Example.Runtime", out PackageAssembly? assembly), Is.True);
		Assert.That(assembly!.TryGetScriptGuid("Example.OldNamespace", "ExampleBehaviour", out UnityGuid guid), Is.True);
		Assert.That(guid, Is.EqualTo(UnityGuid.Parse(RuntimeScriptGuid)));
	}

	[Test]
	public void FoldersEndingWithATildeAreIgnored()
	{
		UnityPackageIndex index = UnityPackageIndex.Load(editorDirectory, new UnityVersion(6000, 3));
		Assert.That(index.TryGetAssembly("Example.Samples", out _), Is.False);
	}
}
