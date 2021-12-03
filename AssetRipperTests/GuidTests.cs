using AssetRipper.Core.Classes.Misc;
using NUnit.Framework;
using System;
using System.Text;

namespace AssetRipper.Tests
{
	public class GuidTests
    {
        [SetUp]
        public void Setup()
        {
        }

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
		/*
		[Test]
		public void ConversionFromSystemGuidToUnityGuidProducesSameString()
		{
			Guid systemGuid = Guid.NewGuid();
			UnityGUID unityGUID = new UnityGUID(systemGuid);
			Assert.AreEqual(systemGuid.ToString().Replace("-",""), unityGUID.ToString());
		}

		[Test]
		public void UnityGuidToStringIsSameAsByteArrayToHex()
		{
			UnityGUID unityGUID = UnityGUID.NewGuid();
			byte[] data = unityGUID.ToByteArray();
			StringBuilder sb = new StringBuilder();
			foreach(byte b in data)
			{
				sb.Append(b.ToString("x2"));
			}
			Assert.AreEqual(sb.ToString(), unityGUID.ToString());
		}
		*/
	}
}