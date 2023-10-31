namespace AssetRipper.Checksum.Tests;

internal class AsciiUtf8ParityTests
{
	[Test]
	public void EnabledTest()
	{
		const string str = "m_Enabled";
		uint utf8 = Crc32Algorithm.HashUTF8(str);
		uint ascii = Crc32Algorithm.HashAscii(str);
		Assert.That(utf8, Is.EqualTo(ascii));
	}
}
