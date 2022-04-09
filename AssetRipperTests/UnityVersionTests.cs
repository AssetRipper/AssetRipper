using AssetRipper.VersionUtilities;
using NUnit.Framework;

namespace AssetRipper.Tests
{
	public class UnityVersionTests
	{
		[Test]
		public void UnityVersionParsesCorrectly()
		{
			string version = "2343.4.5f7";
			UnityVersion expected = new UnityVersion(2343, 4, 5, UnityVersionType.Final, 7);
			Assert.AreEqual(expected, UnityVersion.Parse(version));
		}

		[Test]
		public void UnityVersionParsesDllCorrectly()
		{
			string dllName = "_2343_4_5f7.dll";
			UnityVersion expected = new UnityVersion(2343, 4, 5, UnityVersionType.Final, 7);
			Assert.AreEqual(expected, UnityVersion.ParseFromDllName(dllName));
		}

		[Test]
		public void UnityVersionParsesDllCorrectlyWithoutExtension()
		{
			string dllName = "_2343_4_5f7";
			UnityVersion expected = new UnityVersion(2343, 4, 5, UnityVersionType.Final, 7);
			Assert.AreEqual(expected, UnityVersion.ParseFromDllName(dllName));
		}

		[Test]
		public void UnityVersionParsesDllCorrectlyWithoutLeadingUnderscore()
		{
			string dllName = "2343_4_5f7.dll";
			UnityVersion expected = new UnityVersion(2343, 4, 5, UnityVersionType.Final, 7);
			Assert.AreEqual(expected, UnityVersion.ParseFromDllName(dllName));
		}
	}
}
