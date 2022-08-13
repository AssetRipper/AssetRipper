using AssetRipper.Core.IO;
using AssetRipper.SourceGenerated.Subclasses.FastPropertyName;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;

namespace AssetRipper.Tests
{
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

			Assert.Multiple(() =>
			{
				Assert.IsTrue(dictionary.ContainsKey(1));
				Assert.IsFalse(dictionary.ContainsKey(2));
				Assert.That(dictionary[3], Is.EqualTo(4));
			});
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

			Assert.Multiple(() =>
			{
				Assert.That(dictionary.Count, Is.EqualTo(3));
				Assert.IsTrue(dictionary.ContainsKey(Cast("One")));
				Assert.IsTrue(dictionary.ContainsKey(Cast("Three")));
				Assert.IsFalse(dictionary.ContainsKey(Cast("Four")));
				Assert.That(Cast("Three").GetHashCode(), Is.EqualTo(Cast("Three").GetHashCode()));
				Assert.That(Cast("Three").Equals(Cast("Three")));
				Assert.That(dictionary[Cast("Three")], Is.EqualTo(3));
			});
		}

		[Test]
		public void Utf8StringTests()
		{
			Assert.Multiple(() =>
			{
				Assert.That(Cast(""), Is.EqualTo(Cast("")));
				Assert.That(Cast("One"), Is.EqualTo(Cast("One")));
				Assert.That(Cast("Two"), Is.EqualTo(Cast("Two")));
				Assert.That(Cast("Three"), Is.EqualTo(Cast("Three")));
			});
		}

		private static Utf8String Cast(string str)
		{
			return new() { String = str };
		}

		[Test]
		public void FastPropertyNameDictionaryTests()
		{
			AssetDictionary<FastPropertyName, int> dictionary = new()
			{
				{ MakeFastPropertyName("One"), 1 },
				{ MakeFastPropertyName("Two"), 2 },
				{ MakeFastPropertyName("Three"), 3 }
			};

			Assert.Multiple(() =>
			{
				Assert.That(dictionary.Count, Is.EqualTo(3));
				Assert.IsTrue(dictionary.ContainsKey(MakeFastPropertyName("One")));
				Assert.IsTrue(dictionary.ContainsKey(MakeFastPropertyName("Three")));
				Assert.IsFalse(dictionary.ContainsKey(MakeFastPropertyName("Four")));
				Assert.That(MakeFastPropertyName("Three").GetHashCode(), Is.EqualTo(MakeFastPropertyName("Three").GetHashCode()));
				Assert.That(MakeFastPropertyName("Three").Equals(MakeFastPropertyName("Three")));
				Assert.That(dictionary[MakeFastPropertyName("Three")], Is.EqualTo(3));
			});
		}

		private static FastPropertyName MakeFastPropertyName(string str)
		{
			return new() { NameString = str };
		}
	}
}
