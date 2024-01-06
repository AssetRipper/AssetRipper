using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Subclasses.StaticBatchInfo;

namespace AssetRipper.Tests.Traversal;

internal class DefaultYamlWalkerTests
{
	[TestCaseSource(nameof(GetObjectTypes))]
	public static void SerializedObjectIsConsistent(Type type, string yamlExpectedHyphen, string? yamlExpectedNoHyphen)
	{
		UnityObjectBase asset = AssetCreator.CreateUnsafe(type);
		Assert.Multiple(() =>
		{
			AssertYamlGeneratedAsExpected(new StringYamlWalker(), asset, yamlExpectedHyphen);
			AssertYamlGeneratedAsExpected(new YamlWalkerWithoutHyphens(), asset, yamlExpectedNoHyphen ?? yamlExpectedHyphen);
		});

		static void AssertYamlGeneratedAsExpected(DefaultYamlWalker yamlWalker, UnityObjectBase asset, string yamlExpected)
		{
			string? yamlActual = yamlWalker.AppendEditor(asset, 1).ToString();
			Assert.That(yamlActual, Is.EqualTo(yamlExpected));
		}
	}

	private static IEnumerable<object?[]> GetObjectTypes()
	{
		yield return [typeof(ComponentListObject), ComponentListObject.Yaml, null];
		yield return [typeof(DictionaryObject), DictionaryObject.Yaml, null];
		yield return [typeof(GuidDictionaryObject), GuidDictionaryObject.Yaml, GuidDictionaryObject.YamlWithoutHyphens];
		yield return [typeof(ListObject), ListObject.Yaml, null];
		yield return [typeof(PairListObject), PairListObject.Yaml, null];
		yield return [typeof(PairObject), PairObject.Yaml, null];
		yield return [typeof(ParentObject), ParentObject.Yaml, null];
		yield return [typeof(PrimitiveListObject), PrimitiveListObject.Yaml, null];
		yield return [typeof(SerializedVersionObject), SerializedVersionObject.Yaml, null];
		yield return [typeof(SimpleObject), SimpleObject.Yaml, null];
		yield return [typeof(StringDictionaryObject), StringDictionaryObject.Yaml, StringDictionaryObject.YamlWithoutHyphens];
		yield return [typeof(SubclassObject), SubclassObject.Yaml, null];
		yield return [typeof(StaticSquaredDictionaryObject), StaticSquaredDictionaryObject.Yaml, null];
	}


	private sealed class YamlWalkerWithoutHyphens : StringYamlWalker
	{
		protected override bool UseHyphenInStringDictionary => false;
	}

	[Test]
	public void MonoBehaviourStructureSerializationTest()
	{
		const string yamlExpected = """
			%YAML 1.1
			%TAG !u! tag:unity3d.com,2011:
			--- !u!0 &1
			MonoBehaviour:
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name: 
			  firstSubMesh: 0
			  subMeshCount: 0

			""";
		MonoBehaviour_2017_3 monoBehaviour = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		monoBehaviour.Structure = new StaticBatchInfo();
		string? yamlActual = new StringYamlWalker().AppendRelease(monoBehaviour, 1).ToString();
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
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name: 
			  firstSubMesh: 0
			  subMeshCount: 0
			--- !u!0 &2
			MonoBehaviour:
			  m_GameObject: {m_FileID: 0, m_PathID: 0}
			  m_Enabled: 0
			  m_Script: {m_FileID: 0, m_PathID: 0}
			  m_Name: 
			  firstSubMesh: 0
			  subMeshCount: 0

			""";
		MonoBehaviour_2017_3 monoBehaviour = AssetCreator.CreateUnsafe<MonoBehaviour_2017_3>();
		monoBehaviour.Structure = new StaticBatchInfo();
		string? yamlActual = new StringYamlWalker().AppendRelease(monoBehaviour, 1).AppendRelease(monoBehaviour, 2).ToString();
		Assert.That(yamlActual, Is.EqualTo(yamlExpected));
	}
}
