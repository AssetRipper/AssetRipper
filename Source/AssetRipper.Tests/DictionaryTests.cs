using AssetRipper.Assets.Generics;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Subclasses.FastPropertyName;

namespace AssetRipper.Tests;

public class DictionaryTests
{
	[Test]
	public void SimpleIntegerDictionaryTests()
	{
		AssetDictionary<int, int> dictionary = new()
		{
			{ 1, 2 },
			{ 3, 4 },
			{ 4, 4 }
		};

		using (Assert.EnterMultipleScope())
		{
			Assert.That(dictionary.ContainsKey(1));
			Assert.That(!dictionary.ContainsKey(2));
			Assert.That(dictionary[3], Is.EqualTo(4));
		}
	}

	[Test]
	public void Utf8DictionaryTests()
	{
		AssetDictionary<Utf8String, int> dictionary = new()
		{
			{ Cast("One"), 1 },
			{ Cast("Two"), 2 },
			{ Cast("Three"), 3 }
		};

		using (Assert.EnterMultipleScope())
		{
			Assert.That(dictionary, Has.Count.EqualTo(3));
			Assert.That(dictionary.ContainsKey(Cast("One")));
			Assert.That(dictionary.ContainsKey(Cast("Three")));
			Assert.That(!dictionary.ContainsKey(Cast("Four")));
#pragma warning disable NUnit2009 // The same value has been provided as both the actual and the expected argument
			Assert.That(Cast("Three").GetHashCode(), Is.EqualTo(Cast("Three").GetHashCode()));
#pragma warning restore NUnit2009 // The same value has been provided as both the actual and the expected argument
			Assert.That(Cast("Three").Equals(Cast("Three")));
			Assert.That(dictionary[Cast("Three")], Is.EqualTo(3));
		}
	}

	[Test]
	public void Utf8StringTests()
	{
		using (Assert.EnterMultipleScope())
		{
#pragma warning disable NUnit2009 // The same value has been provided as both the actual and the expected argument
			Assert.That(Cast(""), Is.EqualTo(Cast("")));
			Assert.That(Cast("One"), Is.EqualTo(Cast("One")));
			Assert.That(Cast("Two"), Is.EqualTo(Cast("Two")));
			Assert.That(Cast("Three"), Is.EqualTo(Cast("Three")));
#pragma warning restore NUnit2009 // The same value has been provided as both the actual and the expected argument
		}
	}

	private static Utf8String Cast(string str) => str;

	[Test]
	public void FastPropertyNameDictionaryTests()
	{
		AssetDictionary<FastPropertyName, int> dictionary = new();
		AddFastPropertyName(dictionary, "One", 1);
		AddFastPropertyName(dictionary, "Two", 2);
		AddFastPropertyName(dictionary, "Three", 3);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(dictionary, Has.Count.EqualTo(3));
			Assert.That(dictionary.ContainsKey(MakeFastPropertyName("One")));
			Assert.That(dictionary.ContainsKey(MakeFastPropertyName("Three")));
			Assert.That(!dictionary.ContainsKey(MakeFastPropertyName("Four")));
#pragma warning disable NUnit2009 // The same value has been provided as both the actual and the expected argument
			Assert.That(MakeFastPropertyName("Three").GetHashCode(), Is.EqualTo(MakeFastPropertyName("Three").GetHashCode()));
#pragma warning restore NUnit2009 // The same value has been provided as both the actual and the expected argument
			Assert.That(MakeFastPropertyName("Three").Equals(MakeFastPropertyName("Three")));
			Assert.That(dictionary[MakeFastPropertyName("Three")], Is.EqualTo(3));
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
