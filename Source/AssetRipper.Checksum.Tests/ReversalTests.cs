using System.Text.RegularExpressions;

namespace AssetRipper.Checksum.Tests;

public partial class ReversalTests
{
	[GeneratedRegex("[H-Wh-w]{6}[HJLN]")]
	private static partial Regex ReverseOutputRegex();

	[Test]
	public void TestReversalSymmetryAndRegexMatching()
	{
		for (int i = 0; i < 50; i++)
		{
			uint hash = RandomHash;
			string reversed = Crc32Algorithm.ReverseAscii(hash);
			Assert.Multiple(() =>
			{
				Assert.That(Crc32Algorithm.HashAscii(reversed), Is.EqualTo(hash));
				Assert.That(ReverseOutputRegex().IsMatch(reversed));
			});
		}
	}

	[Test]
	public void ReversalLengthIsSeven()
	{
		string reversed = Crc32Algorithm.ReverseAscii(RandomHash);
		Assert.That(reversed, Has.Length.EqualTo(7));
	}

	[Test]
	public void PrefixedReversal()
	{
		const string prefix = "Prefix_";
		uint hash = RandomHash;
		string reversed = Crc32Algorithm.ReverseAscii(hash, prefix);
		Assert.Multiple(() =>
		{
			Assert.That(Crc32Algorithm.HashAscii(reversed), Is.EqualTo(hash));
			Assert.That(reversed, Does.StartWith(prefix));
		});
	}

	private static uint RandomHash => TestContext.CurrentContext.Random.NextUInt();
}
