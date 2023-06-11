using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetRipper.Primitives.Tests
{
	public class GuidTests
	{
		private const string randomGuidString = "352a5b3897136ed2702a283243520538";
		private const string sequentialGuidString = "0123456789abcdef0fedcba987654321";

		[Test]
		public void MissingReferenceSerializedCorrectly()
		{
			Assert.That(UnityGUID.MissingReference.ToString(), Is.EqualTo("0000000deadbeef15deadf00d0000000"));
		}

		[Test]
		public void ToByteArrayIsConsistentWithConstructorFromByteArray()
		{
			UnityGUID guid = UnityGUID.NewGuid();
			byte[] bytes = guid.ToByteArray();
			UnityGUID fromBytes = new UnityGUID(bytes);
			Assert.That(fromBytes, Is.EqualTo(guid));
			Assert.That(fromBytes.ToString(), Is.EqualTo(guid.ToString()));
		}

		[Test]
		public void ConversionFromSystemGuidToUnityGuidProducesSameString()
		{
			Guid systemGuid = Guid.NewGuid();
			UnityGUID unityGUID = new UnityGUID(systemGuid);
			Assert.That(unityGUID.ToString(), Is.EqualTo(systemGuid.ToString().Replace("-", "")));
		}

		[Test]
		public void IsZeroReturnsTrueForTheZeroGuid()
		{
			UnityGUID unityGUID = new UnityGUID(0, 0, 0, 0);
			Assert.That(unityGUID.IsZero, Is.True);
		}

		[Test]
		public void IsZeroReturnsFalseForRandomGuid()
		{
			UnityGUID unityGUID = UnityGUID.NewGuid();
			Assert.That(unityGUID.IsZero, Is.False);
		}

		[Test]
		public void ParsedGuidOutputsSameString()
		{
			UnityGUID parsedGUID = UnityGUID.Parse(randomGuidString);
			string outputedString = parsedGUID.ToString();
			Assert.That(outputedString, Is.EqualTo(randomGuidString));
		}

		[Test]
		public void ConversionsAreInverses()
		{
			UnityGUID unityGuid = UnityGUID.NewGuid();
			Guid systemGuid = (Guid)unityGuid;
			Assert.That((UnityGUID)systemGuid, Is.EqualTo(unityGuid));
		}

		[Test]
		public void ByteConversionIsItsOwnInverse()
		{
			UnityGUID originalGuid = UnityGUID.NewGuid();
			UnityGUID inverseGuid = new UnityGUID(originalGuid.ToByteArray());
			UnityGUID equivalentGuid = new UnityGUID(inverseGuid.ToByteArray());
			Assert.That(equivalentGuid, Is.EqualTo(originalGuid));
		}

		[Test]
		public void UnityGuidIsTheSameSizeAsSystemGuid()
		{
			Assert.That(Unsafe.SizeOf<UnityGUID>(), Is.EqualTo(Unsafe.SizeOf<Guid>()));
		}

		[Test]
		public void ToBytesMethod()
		{
			Guid originalGuid = Guid.NewGuid();
			UnityGUID tobyteArrayGuid = new UnityGUID(originalGuid.ToByteArray());
			UnityGUID memoryMarshallGuid = new UnityGUID(MemoryMarshal.Cast<Guid, byte>(new ReadOnlySpan<Guid>(in originalGuid)));
			Assert.That(memoryMarshallGuid, Is.EqualTo(tobyteArrayGuid));
		}
	}
}
