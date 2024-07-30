using AssetRipper.Export.UnityProjects;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests.Traversal;

internal class YamlWalkerTests
{
	[Test]
	public void MonoBehaviourTest()
	{
		MonoBehaviour_2017_3 asset = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		new YamlWalker().ExportYamlDocument(asset, 1);
	}

	[Test]
	public void GameObjectTest()
	{
		GameObject_2018_3 asset = AssetCreator.CreateUnsafe<GameObject_2018_3>();
		new YamlWalker().ExportYamlDocument(asset, 1);
	}

	[Test]
	public void AssetBundleTest()
	{
		AssetBundle_2018_3 asset = AssetCreator.CreateUnsafe<AssetBundle_2018_3>();
		new YamlWalker().ExportYamlDocument(asset, 1);
	}
}
