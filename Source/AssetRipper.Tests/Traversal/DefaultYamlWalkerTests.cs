using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects;

namespace AssetRipper.Tests.Traversal;

internal class DefaultYamlWalkerTests
{
	[TestCaseSource(nameof(GetObjectTypes))]
	public static void SerializedObjectIsConsistent(Type type, string yamlExpectedHyphen, string? yamlExpectedNoHyphen)
	{
		UnityObjectBase asset = AssetCreator.CreateUnsafe(type);
		Assert.Multiple(() =>
		{
			AssertYamlGeneratedAsExpected(new DefaultYamlWalker(), asset, yamlExpectedHyphen);
			AssertYamlGeneratedAsExpected(new YamlWalkerWithoutHyphens(), asset, yamlExpectedNoHyphen ?? yamlExpectedHyphen);
		});

		static void AssertYamlGeneratedAsExpected(DefaultYamlWalker yamlWalker, UnityObjectBase asset, string yamlExpected)
		{
			string yamlActual = yamlWalker.AppendEditor(asset, 1).ToString();
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


	private sealed class YamlWalkerWithoutHyphens : DefaultYamlWalker
	{
		protected override bool UseHyphenInStringDictionary => false;
	}
}
