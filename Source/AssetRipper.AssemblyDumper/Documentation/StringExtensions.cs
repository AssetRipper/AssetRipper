namespace AssetRipper.AssemblyDumper.Documentation;

internal static class StringExtensions
{
	public static string EscapeXml(this string str)
	{
		return XmlUtils.EscapeXmlInvalidCharacters(str);
	}
}
