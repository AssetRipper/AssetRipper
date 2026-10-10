using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

public class Unity6000_5CompatibilityTests
{
	private static readonly UnityVersion Version = UnityVersion.Parse("6000.5.10f1");

	[TestCase(129, "PlayerSettings")]
	[TestCase(55, "PhysicsManager")]
	public void NativePlayerSettingsPreserveTheExactEditorFixture(int classID, string name)
	{
		byte[] bytes = ReadFixture(name + ".release.bin");
		var collection = AssetCreator.CreateCollection(Version);
		collection.SetLayout(Version, BuildTarget.StandaloneWin64Player, TransferInstructionFlags.SerializeGameRelease);
		using MonoManager manager = new(_ => { });
		var asset = new GameAssetFactory(manager).ReadAsset(new AssetInfo(collection, 1, classID), new ReadOnlyArraySegment<byte>(bytes), null, []);
		Assert.That(asset, Is.InstanceOf<TypeTreeObject>());
		var settings = (TypeTreeObject)asset!;
		if (classID == 129)
		{
			Assert.That(settings.EditorFields["cloudEnabled"].AsBoolean, Is.True);
			Assert.That(settings.EditorFields["legacyClampBlendShapeWeights"].AsBoolean, Is.True);
			Assert.That(settings.EditorFields["platformRequiresReadableAssets"].AsBoolean, Is.True);
			Assert.That(settings.EditorFields["insecureHttpOption"].AsInt32, Is.EqualTo(2));
			Assert.That(settings.EditorFields["allowedHttpConnections"].AsInt32, Is.EqualTo(2));
		}
		else
		{
			Assert.That(settings.SerializedVersion, Is.EqualTo(24));
			Assert.That(settings.EditorFields.TryGetField("m_ThreadingMode", out _), Is.True);
		}
		using MemoryStream output = new();
		using AssetWriter writer = new(output, collection);
		asset!.WriteRelease(writer);
		Assert.That(output.ToArray(), Is.EqualTo(bytes));
	}

	[Test]
	public void GUIStylePreservesTheExactEditorFixture()
	{
		byte[] bytes = ReadFixture("GUIStyle.editor.bin");
		IUnityAssetBase style = GameAssetFactory.CreateEngineAsset("GUIStyle", Version);
		EndianSpanReader reader = new(bytes, EndianType.LittleEndian);
		style.ReadEditor(ref reader);
		Assert.That(reader.Position, Is.EqualTo(bytes.Length));
		var collection = AssetCreator.CreateCollection(Version);
		using MemoryStream output = new();
		using AssetWriter writer = new(output, collection);
		style.WriteEditor(writer);
		Assert.That(output.ToArray(), Is.EqualTo(bytes));
	}

	// Created by the official 6000.5.10f1 editor in an empty test project.
	// PlayerSettings has distinctive booleans and HTTP values to detect field shifts.
	private static byte[] ReadFixture(string name)
	{
		using Stream input = typeof(Unity6000_5CompatibilityTests).Assembly.GetManifestResourceStream("AssetRipper.Tests.Fixtures.Unity6000_5_10f1." + name)!;
		using MemoryStream output = new();
		input.CopyTo(output);
		return output.ToArray();
	}
}
