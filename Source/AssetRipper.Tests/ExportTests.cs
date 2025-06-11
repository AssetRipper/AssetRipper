using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
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

	private static VirtualFileSystem Export(ProcessedAssetCollection collection, string outputPath = "output", VirtualFileSystem? fileSystem = null)
	{
		fileSystem ??= new();
		new ExportHandler(new()).Export(CreateGameData(collection), outputPath, fileSystem);
		return fileSystem;
	}

	private static GameData CreateGameData(ProcessedAssetCollection collection)
	{
		return new((GameBundle)collection.Bundle, collection.Version, new BaseManager((s) => { }), null);
	}
}
