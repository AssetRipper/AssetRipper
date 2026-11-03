using AssetRipper.Assets.Generics;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Subclasses.FastPropertyName;

namespace AssetRipper.Tests;

public class DictionaryTests
{
	[Test]
	public async Task SimpleIntegerDictionaryTests()
	{
		AssetDictionary<int, int> dictionary = new()
		{
			{ 1, 2 },
			{ 3, 4 },
			{ 4, 4 }
		};


		{
			await Assert.That(dictionary.ContainsKey(1)).IsTrue();
			await Assert.That(!dictionary.ContainsKey(2)).IsTrue();
			await Assert.That(dictionary[3]).IsEqualTo(4);
		}
	}

	[Test]
	public async Task Utf8DictionaryTests()
	{
		AssetDictionary<Utf8String, int> dictionary = new()
		{
			{ Cast("One"), 1 },
			{ Cast("Two"), 2 },
			{ Cast("Three"), 3 }
		};


		{
			await Assert.That(dictionary.Count).IsEqualTo(3);
			await Assert.That(dictionary.ContainsKey(Cast("One"))).IsTrue();
			await Assert.That(dictionary.ContainsKey(Cast("Three"))).IsTrue();
			await Assert.That(!dictionary.ContainsKey(Cast("Four"))).IsTrue();
			await Assert.That(Cast("Three").GetHashCode()).IsEqualTo(Cast("Three").GetHashCode());
			await Assert.That(Cast("Three").Equals(Cast("Three"))).IsTrue();
			await Assert.That(dictionary[Cast("Three")]).IsEqualTo(3);
		}
	}

	[Test]
	public async Task Utf8StringTests()
	{

		{
			await Assert.That(Cast("")).IsEqualTo(Cast(""));
			await Assert.That(Cast("One")).IsEqualTo(Cast("One"));
			await Assert.That(Cast("Two")).IsEqualTo(Cast("Two"));
			await Assert.That(Cast("Three")).IsEqualTo(Cast("Three"));
		}
	}

	private static Utf8String Cast(string str) => str;

	[Test]
	public async Task FastPropertyNameDictionaryTests()
	{
		AssetDictionary<FastPropertyName, int> dictionary = new();
		AddFastPropertyName(dictionary, "One", 1);
		AddFastPropertyName(dictionary, "Two", 2);
		AddFastPropertyName(dictionary, "Three", 3);


		{
			await Assert.That(dictionary.Count).IsEqualTo(3);
			await Assert.That(dictionary.ContainsKey(MakeFastPropertyName("One"))).IsTrue();
			await Assert.That(dictionary.ContainsKey(MakeFastPropertyName("Three"))).IsTrue();
			await Assert.That(!dictionary.ContainsKey(MakeFastPropertyName("Four"))).IsTrue();
			await Assert.That(MakeFastPropertyName("Three").GetHashCode()).IsEqualTo(MakeFastPropertyName("Three").GetHashCode());
			await Assert.That(MakeFastPropertyName("Three").Equals(MakeFastPropertyName("Three"))).IsTrue();
			await Assert.That(dictionary[MakeFastPropertyName("Three")]).IsEqualTo(3);
		}

		static FastPropertyName MakeFastPropertyName(string str) => new() { Name = str };

		static void AddFastPropertyName(AssetDictionary<FastPropertyName, int> dictionary, string str, int value)
		{
			AssetPair<FastPropertyName, int> pair = dictionary.AddNew();
			pair.Key.Name = str;
			pair.Value = value;
		}
	}
}