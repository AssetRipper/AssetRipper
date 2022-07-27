using AssetRipper.Core.Classes.Misc;
using System;
using System.Text;

namespace AssetRipper.Tests
{
	public class GuidTests
	{
		private const string randomGuidString = "352a5b3897136ed2702a283243520538";
		private const string sequentialGuidString = "0123456789abcdef0fedcba987654321";

		[Test]
		public void MissingReferenceSerializedCorrectly()
		{
			Assert.AreEqual("0000000deadbeef15deadf00d0000000", UnityGUID.MissingReference.ToString());
		}

		[Test]
		public void ToByteArrayIsConsistentWithConstructorFromByteArray()
		{
			UnityGUID guid = UnityGUID.NewGuid();
			byte[] bytes = guid.ToByteArray();
			UnityGUID fromBytes = new UnityGUID(bytes);
			Assert.AreEqual(guid, fromBytes);
			Assert.AreEqual(guid.ToString(), fromBytes.ToString());
		}

		[Test]
		public void ConversionFromSystemGuidToUnityGuidProducesSameString()
		{
			Guid systemGuid = Guid.NewGuid();
			UnityGUID unityGUID = new UnityGUID(systemGuid);
			Assert.AreEqual(systemGuid.ToString().Replace("-", ""), unityGUID.ToString());
		}

		[Test]
		public void IsZeroReturnsTrueForTheZeroGuid()
		{
			UnityGUID unityGUID = new UnityGUID(0, 0, 0, 0);
			Assert.IsTrue(unityGUID.IsZero);
		}

		[Test]
		public void IsZeroReturnsFalseForRandomGuid()
		{
			UnityGUID unityGUID = UnityGUID.NewGuid();
			Assert.IsFalse(unityGUID.IsZero);
		}

		[Test]
		public void ParsedGuidOutputsSameString()
		{
			UnityGUID parsedGUID = UnityGUID.Parse(randomGuidString);
			string outputedString = parsedGUID.ToString();
			Assert.AreEqual(randomGuidString, outputedString);
		}

		[Test]
		public void ConversionsAreInverses()
		{
			UnityGUID unityGuid = UnityGUID.NewGuid();
			Guid systemGuid = (Guid)unityGuid;
			Assert.AreEqual(unityGuid, (UnityGUID)systemGuid);
		}

		[Test]
		public void ByteConversionIsItsOwnInverse()
		{
			UnityGUID originalGuid = UnityGUID.NewGuid();
			UnityGUID inverseGuid = new UnityGUID(originalGuid.ToByteArray());
			UnityGUID equivalentGuid = new UnityGUID(inverseGuid.ToByteArray());
			Assert.AreEqual(originalGuid, equivalentGuid);
		}

		private static string GetLongRandomString(int numSetsOf32Characters = 4)
		{
			StringBuilder sb = new(numSetsOf32Characters * 32);
			for (int i = 0; i < numSetsOf32Characters; i++)
			{
				sb.Append(Guid.NewGuid().ToString());
			}
			return sb.ToString();
		}
	}
}
