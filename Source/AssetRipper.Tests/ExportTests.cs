using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
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

		VirtualFileSystem fileSystem = new();

		Export(collection, "output", fileSystem);

		Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/MonoBehaviour/Name.asset"));
	}

	[Test]
	public void NamelessScriptableObjectIsExported()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2022);

		collection.CreateMonoBehaviour();

		VirtualFileSystem fileSystem = new();

		Export(collection, "output", fileSystem);

		Assert.That(fileSystem.File.Exists("/output/ExportedProject/Assets/MonoBehaviour/MonoBehaviour.asset"));
	}

	private static void Export(ProcessedAssetCollection collection, string outputPath, VirtualFileSystem fileSystem)
	{
		new ExportHandler(new()).Export(CreateGameData(collection), outputPath, fileSystem);
	}

	private static GameData CreateGameData(ProcessedAssetCollection collection)
	{
		return new((GameBundle)collection.Bundle, collection.Version, new BaseManager((s) => { }), null);
	}
}
