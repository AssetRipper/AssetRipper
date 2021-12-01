using AssetRipper.Core.Classes.Misc;
using NUnit.Framework;

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
	}
}