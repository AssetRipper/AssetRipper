using AsmResolver.DotNet;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.Processing.ScriptableObject;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using NUnit.Framework.Internal;

namespace AssetRipper.Tests;

internal class ExportTests
{
	[Test]
	public void NamedScriptableObjectIsExported()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		IMonoBehaviour monoBehaviour = collection.CreateMonoBehaviour();
		monoBehaviour.Name = "Name";

		VirtualFileSystem fileSystem = Export(collection);

		Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/MonoBehaviour/Name.asset"));
	}

	[Test]
	public void NamelessScriptableObjectIsExported()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		collection.CreateMonoBehaviour();

		VirtualFileSystem fileSystem = Export(collection);

		Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/MonoBehaviour/MonoBehaviour.asset"));
	}

	[Test]
	public void ScriptableObjectGroupIsExported_1()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		IMonoBehaviour behaviour1 = collection.CreateMonoBehaviour();
		IMonoBehaviour behaviour2 = collection.CreateMonoBehaviour();
		ScriptableObjectGroup group = collection.CreateAsset(default, (assetInfo) => new ScriptableObjectGroup(assetInfo, behaviour1));
		group.Children.Add(behaviour2);
		group.SetMainAsset();

		Assert.DoesNotThrow(() => Export(collection));
	}

	[Test]
	public void ScriptableObjectGroupIsExported_2()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		IMonoBehaviour behaviour1 = collection.CreateMonoBehaviour();
		IMonoBehaviour behaviour2 = collection.CreateMonoBehaviour();
		ScriptableObjectGroup group = collection.CreateAsset(default, (assetInfo) => new ScriptableObjectGroup(assetInfo, behaviour2));
		group.Children.Add(behaviour1);
		group.SetMainAsset();

		Assert.DoesNotThrow(() => Export(collection));
	}

	[Test]
	public void ScriptableObjectGroupIsExported_3()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		IMonoBehaviour behaviour1 = collection.CreateMonoBehaviour();
		ScriptableObjectGroup group = collection.CreateAsset(default, (assetInfo) => new ScriptableObjectGroup(assetInfo, behaviour1));
		IMonoBehaviour behaviour2 = collection.CreateMonoBehaviour();
		group.Children.Add(behaviour2);
		group.SetMainAsset();

		Assert.DoesNotThrow(() => Export(collection));
	}

	[Test]
	public void EmptyMeshIsExported()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		collection.CreateMesh();

		VirtualFileSystem fileSystem = Export(collection);

		Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/Mesh/Mesh.asset"));
	}

	[Test]
	public void CompressedMeshIsExported()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		IMesh mesh = collection.CreateMesh();
		mesh.FillWithCompressedMeshData(MeshData.CreateTriangleMesh());

		VirtualFileSystem fileSystem = Export(collection);

		Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/Mesh/Mesh.asset"));
	}

	static readonly (string AssemblyName, string AssemblyGuid)[] AssemblyGuidTestCases =
	[
		("UnityEngine.UI", "f5f67c52d1564df4a8936ccd202a3bd8"),
	];
	static readonly UnityVersion[] AssemblyGuidTestVersions =
	[
		UnityVersion.V_5_3,
		UnityVersion.V_5_4,
		UnityVersion.V_5_5,
		UnityVersion.V_5_6,
		UnityVersion.V_2017,
		UnityVersion.V_2018,
	];
	[Theory]
	public void AssembliesHaveCorrectGuid([ValueSource(nameof(AssemblyGuidTestCases))] (string, string) pair, [ValueSource(nameof(AssemblyGuidTestVersions))] UnityVersion version)
	{
		(string assemblyName, string assemblyGuid) = pair;
		MonoManager assemblyManager = CreateAssemblyManager();
		const string TypeName = "ExampleBehaviour";
		{
			assemblyManager.Load(typeof(UnityEngine.Object).Assembly.Location, LocalFileSystem.Instance);
			ModuleDefinition unityEngineModule = assemblyManager.GetAssemblies().Single().ManifestModule!;
			TypeDefinition monoBehaviourType = unityEngineModule.TopLevelTypes.Single(t => t.Name == "MonoBehaviour");
			AssemblyDefinition newAssembly = new(assemblyName, new());
			ModuleDefinition newModule = new(assemblyName, unityEngineModule.AssemblyReferences.First(a => a.IsCorLib));
			newAssembly.Modules.Add(newModule);
			TypeDefinition newType = new(null, TypeName, monoBehaviourType.Attributes, newModule.DefaultImporter.ImportType(monoBehaviourType));
			newModule.TopLevelTypes.Add(newType);
			MemoryStream stream = new();
			newModule.Write(stream);
			stream.Position = 0;
			assemblyManager.Read(stream, $"{assemblyName}.dll");
		}

		ProcessedAssetCollection collection = AssetCreator.CreateCollection(version);

		IMonoScript monoScript = collection.CreateMonoScript();
		monoScript.ClassName_R = TypeName;
		monoScript.AssemblyName = assemblyName;

		IMonoBehaviour monoBehaviour = collection.CreateMonoBehaviour();
		monoBehaviour.ScriptP = monoScript;
		monoBehaviour.Name = "TestBehaviour";

		VirtualFileSystem fileSystem = Export(collection, assemblyManager: assemblyManager);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/MonoBehaviour/TestBehaviour.asset"));
			Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/MonoBehaviour/TestBehaviour.asset.meta"));
		}
		Assert.That(fileSystem.File.ReadAllText("/output/ExportedProject/Assets/MonoBehaviour/TestBehaviour.asset"), Does.Contain(assemblyGuid));
	}

	private static VirtualFileSystem Export(ProcessedAssetCollection collection, string outputPath = "output", IAssemblyManager? assemblyManager = null, VirtualFileSystem? fileSystem = null)
	{
		fileSystem ??= new();
		new ExportHandler(new()).Export(CreateGameData(collection, assemblyManager), outputPath, fileSystem);
		return fileSystem;
	}

	private static GameData CreateGameData(ProcessedAssetCollection collection, IAssemblyManager? assemblyManager = null)
	{
		return new((GameBundle)collection.Bundle, collection.Version, assemblyManager ?? new BaseManager((s) => { }), null);
	}

	private static MonoManager CreateAssemblyManager()
	{
		return new MonoManager((str) => { });
	}
}
