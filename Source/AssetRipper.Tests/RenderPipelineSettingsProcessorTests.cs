using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.Processing.Editor;
using AssetRipper.SourceGenerated.Classes.ClassID_0;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_30;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;
using AssetRipper.Tests.Fixtures;

namespace AssetRipper.Tests;

public class RenderPipelineSettingsProcessorTests
{
	[TestCase(true)]
	[TestCase(false)]
	public void OnlyPlayerSettingsGainAnEditorRegistration(bool isRelease)
	{
		using MonoManager manager = CreateManager();
		ProcessedAssetCollection collection = CreateCollection(isRelease);
		IGraphicsSettings settings = collection.CreateAsset(30, GraphicsSettings.Create);
		IMonoBehaviour globalSettings = CreateGlobalSettings(collection);
		settings.CurrentRenderPipelineGlobalSettingsP = globalSettings;
		GameData gameData = new((GameBundle)collection.Bundle, collection.Version, manager, null);

		new RenderPipelineSettingsProcessor().Process(gameData);
		new RenderPipelineSettingsProcessor().Process(gameData);

		Assert.That(settings.RenderPipelineGlobalSettingsMap!.Count, Is.EqualTo(isRelease ? 1 : 0));
		if (isRelease)
		{
			Assert.That(settings.RenderPipelineGlobalSettingsMap.GetKey(0).String, Is.EqualTo(typeof(RenderPipelineFixture).FullName));
			Assert.That(settings.RenderPipelineGlobalSettingsMap.GetValue(0).TryGetAsset(collection, out IObject? asset), Is.True);
			Assert.That(asset, Is.SameAs(globalSettings));
		}
	}

	[Test]
	public void ExistingEditorRegistrationsArePreserved()
	{
		using MonoManager manager = CreateManager();
		ProcessedAssetCollection collection = CreateCollection(true);
		IGraphicsSettings settings = collection.CreateAsset(30, GraphicsSettings.Create);
		settings.CurrentRenderPipelineGlobalSettingsP = CreateGlobalSettings(collection);
		IMonoBehaviour existingSettings = CreateGlobalSettings(collection);
		AssetPair<Utf8String, PPtr_Object_5> pair = settings.RenderPipelineGlobalSettingsMap!.AddNew();
		pair.Key = typeof(RenderPipelineFixture).FullName!;
		pair.Value.SetAsset(collection, existingSettings);
		GameData gameData = new((GameBundle)collection.Bundle, collection.Version, manager, null);

		new RenderPipelineSettingsProcessor().Process(gameData);

		Assert.That(settings.RenderPipelineGlobalSettingsMap.Count, Is.EqualTo(1));
		Assert.That(pair.Value.TryGetAsset(collection, out IObject? asset), Is.True);
		Assert.That(asset, Is.SameAs(existingSettings));
	}

	[Test]
	public void MissingPlayerSettingsDoNotCreateARegistration()
	{
		using MonoManager manager = CreateManager();
		ProcessedAssetCollection collection = CreateCollection(true);
		IGraphicsSettings settings = collection.CreateAsset(30, GraphicsSettings.Create);
		GameData gameData = new((GameBundle)collection.Bundle, collection.Version, manager, null);

		new RenderPipelineSettingsProcessor().Process(gameData);

		Assert.That(settings.RenderPipelineGlobalSettingsMap, Is.Empty);
	}

	private static MonoManager CreateManager()
	{
		MonoManager manager = new(_ => { });
		manager.Load(typeof(RenderPipelineSettingsFixture).Assembly.Location, LocalFileSystem.Instance);
		return manager;
	}

	private static ProcessedAssetCollection CreateCollection(bool isRelease)
	{
		UnityVersion version = UnityVersion.Parse("6000.5.10f1");
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(version);
		collection.SetLayout(version, BuildTarget.StandaloneWin64Player, isRelease ? TransferInstructionFlags.SerializeGameRelease : default);
		return collection;
	}

	private static IMonoBehaviour CreateGlobalSettings(ProcessedAssetCollection collection)
	{
		IMonoScript script = collection.CreateAsset(115, MonoScript.Create);
		script.AssemblyName = typeof(RenderPipelineSettingsFixture).Assembly.GetName().Name!;
		script.Namespace = typeof(RenderPipelineSettingsFixture).Namespace!;
		script.ClassName_R = nameof(RenderPipelineSettingsFixture);
		IMonoBehaviour settings = collection.CreateAsset(114, MonoBehaviour.Create);
		settings.ScriptP = script;
		return settings;
	}
}
