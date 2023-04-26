using AssetRipper.Export.UnityProjects.Scripts;
using System.Text;

namespace AssetRipper.Tests;

internal class ScriptHashingTests
{
	[TestCase("AssetRipper.Examples", "ExampleBehaviour", -218309758)]
	[TestCase("UnityEngine", "Component", 468326297)]
	[TestCase("UnityEngine", "GameObject", -1741091355)]
	[TestCase("UnityEngine", "MonoBehaviour", 888015496)]
	[TestCase("UnityEngine", "Object", 350709172)]
	public static void VerifyCorrectness(string @namespace, string name, int expected)
	{
		int calculated = ScriptHashing.CalculateScriptFileID(@namespace, name);
		Assert.That(calculated, Is.EqualTo(expected));
	}

	[TestCaseSource(nameof(GetRandomFullNames), new object[] { 10 })]
	public static void VerifyConsistency(string @namespace, string name)
	{
		int value1 = ScriptHashing.CalculateScriptFileID(@namespace, name);
		int value2 = ScriptHashing.CalculateScriptFileID(Encoding.UTF8.GetBytes(@namespace), Encoding.UTF8.GetBytes(name));
		Assert.That(value2, Is.EqualTo(value1));
	}

	private static IEnumerable<string[]> GetRandomFullNames(int count)
	{
		for (int i = 0; i < count; i++)
		{
			string @namespace = GetRandomString(5, 50);
			string className = GetRandomString(5, 15);
			yield return new[] { @namespace, className };
		}
	}

	private static string GetRandomString(int minLength, int maxLength)
	{
		int length = TestContext.CurrentContext.Random.Next(minLength, maxLength);
		return TestContext.CurrentContext.Random.GetString(length);
	}
}
