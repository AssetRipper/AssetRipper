using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.StaticBatchInfo;
using AssetRipper.Yaml;
using System.Globalization;

namespace AssetRipper.Tests.Traversal;

internal class DefaultYamlWalkerTests
{
	[TestCaseSource(nameof(GetObjectTypes))]
	[Ignore("Not investigated")]
	public static void SerializedObjectIsConsistent(Type type, string yamlExpectedHyphen, string? yamlExpectedNoHyphen)
	{
		UnityObjectBase asset = AssetCreator.CreateUnsafe(type);
		using (Assert.EnterMultipleScope())
		{
			AssertYamlGeneratedAsExpected(new DefaultYamlWalker(), asset, yamlExpectedHyphen);
			AssertYamlGeneratedAsExpected(new YamlWalkerWithoutHyphens(), asset, /*yamlExpectedNoHyphen ??*/ yamlExpectedHyphen);
		}

		static void AssertYamlGeneratedAsExpected(YamlWalker yamlWalker, IUnityObjectBase asset, string yamlExpected)
		{
			string yamlActual = GenerateYaml(yamlWalker, asset);
			Assert.That(yamlActual, Is.EqualTo(yamlExpected));
		}
	}

	private static string GenerateYaml(YamlWalker yamlWalker, IUnityObjectBase asset)
	{
		return GenerateYaml(yamlWalker, [(asset, 1)]);
	}

	private static string GenerateYaml(YamlWalker yamlWalker, ReadOnlySpan<(IUnityObjectBase, long)> assets)
	{
		using StringWriter stringWriter = new(CultureInfo.InvariantCulture) { NewLine = "\n" };
		YamlWriter writer = new();
		writer.WriteHead(stringWriter);
		foreach ((IUnityObjectBase asset, long exportID) in assets)
		{
			YamlDocument document = yamlWalker.ExportYamlDocument(asset, exportID);
			writer.WriteDocument(document);
		}
		writer.WriteTail(stringWriter);
		return stringWriter.ToString();
	}

	private static object?[][] GetObjectTypes() =>
	[
		[typeof(ComponentListObject), ComponentListObject.Yaml, null],
		[typeof(DictionaryObject), DictionaryObject.Yaml, null],
		[typeof(GuidDictionaryObject), GuidDictionaryObject.Yaml, GuidDictionaryObject.YamlWithoutHyphens],
		[typeof(ListObject), ListObject.Yaml, null],
		[typeof(PairListObject), PairListObject.Yaml, null],
		[typeof(PairObject), PairObject.Yaml, null],
		[typeof(ParentObject), ParentObject.Yaml, null],
		[typeof(PrimitiveListObject), PrimitiveListObject.Yaml, null],
		[typeof(SerializedVersionObject), SerializedVersionObject.Yaml, null],
		[typeof(SimpleObject), SimpleObject.Yaml, null],
		[typeof(StringDictionaryObject), StringDictionaryObject.Yaml, StringDictionaryObject.YamlWithoutHyphens],
		[typeof(SubclassObject), SubclassObject.Yaml, null],
		[typeof(StaticSquaredDictionaryObject), StaticSquaredDictionaryObject.Yaml, null],
	];

	private class DefaultYamlWalker : YamlWalker
	{
		public override YamlNode CreateYamlNodeForPPtr<TAsset>(PPtr<TAsset> pptr)
		{
			return new YamlMappingNode(MappingStyle.Flow)
			{
				{ "m_FileID", pptr.FileID },
				{ "m_PathID", pptr.PathID },
			};
		}
	}

	private sealed class YamlWalkerWithoutHyphens : DefaultYamlWalker
	{
	}

	[Test]
	public void MonoBehaviourStructureSerializationTest()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			MonoBehaviour:
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_EditorHideFlags: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_EditorClassIdentifier:
			  firstSubMesh: 0
			  subMeshCount: 0

			""";
		MonoBehaviour_2017_3 monoBehaviour = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		monoBehaviour.Structure = new StaticBatchInfo();
		string yamlActual = GenerateYaml(new DefaultYamlWalker(), monoBehaviour);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}

	[Test]
	public void MultipleMonoBehaviourStructureSerializationTest()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			MonoBehaviour:
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_EditorHideFlags: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_EditorClassIdentifier:
			  firstSubMesh: 0
			  subMeshCount: 0
			--- !u!0 &2
			MonoBehaviour:
			  m_ObjectHideFlags: 0
			  m_PrefabParentObject: {m_FileID: 0, m_PathID: 0}
			  m_PrefabInternal: {m_FileID: 0, m_PathID: 0}
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_EditorHideFlags: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name:
			  m_EditorClassIdentifier:
			  firstSubMesh: 0
			  subMeshCount: 0

			""";
		MonoBehaviour_2017_3 monoBehaviour = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		monoBehaviour.Structure = new StaticBatchInfo();
		string yamlActual = GenerateYaml(new DefaultYamlWalker(), [(monoBehaviour, 1), (monoBehaviour, 2)]);
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}
}
