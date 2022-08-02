using AssetRipper.Core.Utils;
using System.Text.RegularExpressions;

namespace AssetRipper.Tests
{
	public class CrcTests
	{
		private readonly Regex reverseOutputRegex = new Regex(@"[H-Wh-w]{6}[HJLN]");

		[Test]
		public void TestReversalSymmetryAndRegexMatching()
		{
			foreach (uint checksum in new RandomStructEnumerable<uint>(1000))
			{
				string reversed = CrcUtils.ReverseDigestAscii(checksum);
				Assert.Multiple(() =>
				{
					Assert.That(CrcUtils.CalculateDigestAscii(reversed), Is.EqualTo(checksum));
					Assert.That(reverseOutputRegex.IsMatch(reversed));
				});
			}
		}

		[Test]
		public void ReversalLengthIsSeven()
		{
			string reversed = CrcUtils.ReverseDigestAscii(0x4df325ab);
			Assert.That(reversed.Length, Is.EqualTo(7));
		}
	}
}
