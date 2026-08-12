using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

/// <summary>
/// Chinese Unity builds (versions with a "c" type, eg 2022.3.20f1c1) are based on a much later engine than their
/// version number suggests, so their serialized layouts can differ from international builds with the same version number.
/// </summary>
/// <remarks>
/// The test data is the raw object data from a game built with Unity 2022.3.20f1c1.
/// </remarks>
internal class ChinaVersionTests
{
	private static readonly UnityVersion China2022_3_20 = new(2022, 3, 20, UnityVersionType.China, 1);

	[Test]
	public void ChinaBuildSettingsReadsWithAuthToken()
	{
		ProcessedAssetCollection collection = CreateChinaCollection();
		GameAssetFactory factory = new(new BaseManager(_ => { }));

		IUnityObjectBase asset = factory.ReadAsset(new AssetInfo(collection, 1, 141), Convert.FromHexString(BuildSettingsHex), null);

		Assert.That(asset, Is.InstanceOf<BuildSettings_2022_3_52>());
		BuildSettings_2022_3_52 buildSettings = (BuildSettings_2022_3_52)asset!;
		Assert.That(buildSettings.Scenes.Count, Is.EqualTo(3));
		Assert.That(buildSettings.Scenes[0].String, Is.EqualTo("Assets/Scenes/VillageScene.unity"));
		Assert.That(buildSettings.AuthToken!.String, Is.EqualTo("2483993"));
		Assert.That(buildSettings.GraphicsAPIs.Count, Is.EqualTo(1));
		Assert.That(buildSettings.GraphicsAPIs[0], Is.EqualTo(2));
	}

	[Test]
	public void ChinaUnityConnectSettingsReadsWithChinaUrls()
	{
		ProcessedAssetCollection collection = CreateChinaCollection();
		GameAssetFactory factory = new(new BaseManager(_ => { }));

		IUnityObjectBase asset = factory.ReadAsset(new AssetInfo(collection, 1, 310), Convert.FromHexString(UnityConnectSettingsHex), null);

		Assert.That(asset, Is.InstanceOf<TypeTreeObject>());
		TypeTreeObject typeTree = (TypeTreeObject)asset!;
		Assert.That(typeTree.ReleaseFields["m_Enabled"].AsBoolean, Is.True);
		Assert.That(typeTree.ReleaseFields["m_EventOldUrl"].AsString, Is.EqualTo("https://api.uca.cloud.unity3d.com/v1/events"));
		Assert.That(typeTree.ReleaseFields["m_EventUrl"].AsString, Is.EqualTo("https://cdp.cloud.unity3d.com/v1/events"));
		Assert.That(typeTree.ReleaseFields["m_ConfigUrl"].AsString, Is.EqualTo("https://config.uca.cloud.unity3d.com"));
		Assert.That(typeTree.ReleaseFields["m_DashboardUrl"].AsString, Is.EqualTo("https://dashboard.unity3d.com"));
		Assert.That(typeTree.ReleaseFields["m_ChinaEventUrl"].AsString, Is.EqualTo("https://cdp.cloud.unity.cn/v1/events"));
		Assert.That(typeTree.ReleaseFields["m_ChinaConfigUrl"].AsString, Is.EqualTo("https://cdp.cloud.unity.cn/config"));
		Assert.That(typeTree.ReleaseFields["m_TestInitMode"].AsInt32, Is.Zero);
	}

	private static ProcessedAssetCollection CreateChinaCollection()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(China2022_3_20);
		collection.SetLayout(China2022_3_20, BuildTarget.NoTarget, TransferInstructionFlags.SerializeGameRelease);
		return collection;
	}

	private const string BuildSettingsHex =
		"03000000200000004173736574732F5363656E65732F56696C6C616765536365" +
		"6E652E756E6974791D0000004173736574732F5363656E65732F47616D655363" +
		"656E652E756E697479000000200000004173736574732F5363656E65732F4C6F" +
		"6164696E675363656E652E756E69747900000000000000000000000000010000" +
		"0100000001010100010100000D000000323032322E332E323066316331000000" +
		"0700000032343833393933000100000002000000";

	private const string UnityConnectSettingsHex =
		"010000002B00000068747470733A2F2F6170692E7563612E636C6F75642E756E" +
		"69747933642E636F6D2F76312F6576656E7473002700000068747470733A2F2F" +
		"6364702E636C6F75642E756E69747933642E636F6D2F76312F6576656E747300" +
		"2400000068747470733A2F2F636F6E6669672E7563612E636C6F75642E756E69" +
		"747933642E636F6D1D00000068747470733A2F2F64617368626F6172642E756E" +
		"69747933642E636F6D0000002400000068747470733A2F2F6364702E636C6F75" +
		"642E756E6974792E636E2F76312F6576656E74732100000068747470733A2F2F" +
		"6364702E636C6F75642E756E6974792E636E2F636F6E66696700000000000000" +
		"2200000068747470733A2F2F706572662D6576656E74732E636C6F75642E756E" +
		"6974792E636E0000000000000A00000000000000010001000001000000000000" +
		"00000000";
}
