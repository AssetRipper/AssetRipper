using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Primitives;
using AssetRipper.Processing.Prefabs;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.Yaml;
using NUnit.Framework.Internal;

namespace AssetRipper.Tests;

public class StrippedAssetTests
{
	[Test]
	public void IsStrippedReturnsTrueForStrippedAsset()
	{
		ProcessedAssetCollection collection = CreateCollection(new UnityVersion(2020, 1));
		IGameObject root = collection.CreateGameObject();
		IPrefabInstance prefab = collection.CreatePrefabInstance();
		PrefabHierarchyObject hierarchy = collection.CreateAsset(-1, (assetInfo) => new PrefabHierarchyObject(assetInfo, root, prefab));
		hierarchy.GameObjects.Add(root);
		hierarchy.SetMainAsset();

		hierarchy.StrippedAssets.Add(root);

		Assert.That(root.IsStripped(), Is.True);
	}

	[Test]
	public void StrippedGameObjectYamlContent()
	{
		ProcessedAssetCollection collection = CreateCollection(new UnityVersion(2020, 1));
		SceneDefinition scene = SceneDefinition.FromName("TestScene");
		scene.AddCollection(collection);
		IGameObject gameObject = collection.CreateGameObject();
		SceneHierarchyObject hierarchy = collection.CreateAsset(-1, (assetInfo) => new SceneHierarchyObject(assetInfo, scene));
		hierarchy.GameObjects.Add(gameObject);
		hierarchy.SetMainAsset();

		hierarchy.StrippedAssets.Add(gameObject);

		string yaml = GetYamlForAsset(gameObject);
		const string expected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!1 &1 stripped
			GameObject:
			  m_CorrespondingSourceObject: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 18}
			  m_PrefabInstance: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 1001}
			  m_PrefabAsset: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 1001480554}

			""";

		Assert.That(yaml.ReplaceLineEndings(), Is.EqualTo(expected.ReplaceLineEndings()));
	}

	[Test]
	public void StrippedMonoBehaviourYamlContent()
	{
		ProcessedAssetCollection collection = CreateCollection(new UnityVersion(2020, 1));
		SceneDefinition scene = SceneDefinition.FromName("TestScene");
		scene.AddCollection(collection);
		IMonoBehaviour monoBehaviour = collection.CreateMonoBehaviour();
		SceneHierarchyObject hierarchy = collection.CreateAsset(-1, (assetInfo) => new SceneHierarchyObject(assetInfo, scene));
		hierarchy.Components.Add(monoBehaviour);
		hierarchy.SetMainAsset();

		hierarchy.StrippedAssets.Add(monoBehaviour);

		string yaml = GetYamlForAsset(monoBehaviour);
		const string expected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!114 &1 stripped
			MonoBehaviour:
			  m_CorrespondingSourceObject: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 18}
			  m_PrefabInstance: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 1001}
			  m_PrefabAsset: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 1001480554}
			  m_GameObject: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 1}
			  m_Enabled: 0
			  m_EditorHideFlags: 0
			  m_Script: {m_FileID: 0, m_PathID: 0, m_TargetClassID: 115}
			  m_Name:
			  m_EditorClassIdentifier:

			""";

		// Note: The source yaml from Unity had a trailing space after the colon for m_Name and m_EditorClassIdentifier.
		// AssetRipper does not currently add a trailing space, which might be a bug in its serialization.
		// However, that's unrelated to stripping, and there doesn't seem to be any issues arising from the difference.

		Assert.That(yaml.ReplaceLineEndings(), Is.EqualTo(expected.ReplaceLineEndings()));
	}

	private static string GetYamlForAsset(IUnityObjectBase asset)
	{
		using StringWriter writer = new();
		YamlWriter yamlWriter = new();
		yamlWriter.WriteHead(writer);
		YamlWalker walker = new();
		yamlWriter.WriteDocument(walker.ExportYamlDocument(asset, 1));
		yamlWriter.WriteTail(writer);
		return writer.ToString();
	}

	private static ProcessedAssetCollection CreateCollection(UnityVersion version) => new GameBundle().AddNewProcessedCollection("Collection", version);
}
