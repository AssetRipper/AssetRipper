using AssetRipper.Export.UnityProjects.Scripts;
using System.Text;

namespace AssetRipper.Tests;

internal class ScriptHashingTests
{
	[Test]
	[Arguments("AssetRipper.Examples", "ExampleBehaviour", -218309758)]
	[Arguments("UnityEngine", "Component", 468326297)]
	[Arguments("UnityEngine", "GameObject", -1741091355)]
	[Arguments("UnityEngine", "MonoBehaviour", 888015496)]
	[Arguments("UnityEngine", "Object", 350709172)]
	public async Task VerifyCorrectness(string @namespace, string name, int expected)
	{
		int calculated = ScriptHashing.CalculateScriptFileID(@namespace, name);
		await Assert.That(calculated).IsEqualTo(expected);
	}

	[Test]
	[MethodDataSource(nameof(GetRandomFullNames))]
	public async Task VerifyConsistency(string @namespace, string name)
	{
		int value1 = ScriptHashing.CalculateScriptFileID(@namespace, name);
		int value2 = ScriptHashing.CalculateScriptFileID(Encoding.UTF8.GetBytes(@namespace), Encoding.UTF8.GetBytes(name));
		await Assert.That(value2).IsEqualTo(value1);
	}

	public static IEnumerable<(string Namespace, string Name)> GetRandomFullNames()
	{
		const int count = 100;
		Random random = new(42);
		for (int i = 0; i < count; i++)
		{
			string @namespace = GetRandomString(random, 5, 50);
			string className = GetRandomString(random, 5, 15);
			yield return (@namespace, className);
		}
	}

	private static string GetRandomString(Random random, int minLength, int maxLength)
	{
		const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		int length = random.Next(minLength, maxLength);
		char[] characters = new char[length];
		for (int i = 0; i < characters.Length; i++)
		{
			characters[i] = Characters[random.Next(Characters.Length)];
		}
		return new string(characters);
	}
}