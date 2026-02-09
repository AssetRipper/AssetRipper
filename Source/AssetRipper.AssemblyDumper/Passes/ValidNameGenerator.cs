using System.Text.RegularExpressions;

internal static partial class ValidNameGenerator
{
	/// <summary>
	/// Fixes the string to be a valid field name
	/// </summary>
	/// <param name="originalName"></param>
	/// <returns>A new string with the valid content</returns>
	public static string GetValidFieldName(string originalName)
	{
		if (string.IsNullOrWhiteSpace(originalName))
		{
			throw new ArgumentException("Nodes cannot have a null or whitespace name", nameof(originalName));
		}
		string result = originalName.ReplaceBadCharacters();
		if (char.IsDigit(result[0]) || !result.StartsWith("m_", StringComparison.Ordinal))
		{
			result = "m_" + result;
		}
		if (char.IsLower(result[2]))
		{
			string remaining = result.Length > 3 ? result.Substring(3) : "";
			result = $"m_{char.ToUpperInvariant(result[2])}{remaining}";
		}
		return result;
	}

	/// <summary>
	/// Fixes the string to be a valid type name
	/// </summary>
	/// <param name="originalName"></param>
	/// <returns>A new string with the valid content</returns>
	public static string GetValidTypeName(string originalName)
	{
		if (string.IsNullOrWhiteSpace(originalName))
		{
			throw new ArgumentException("Nodes cannot have a null or whitespace type name", nameof(originalName));
		}
		string result = originalName.ReplaceBadCharacters();
		if (char.IsDigit(result[0]))
		{
			result = "_" + result;
		}
		if (char.IsLower(result[0]) && result.Length > 1)
		{
			result = char.ToUpperInvariant(result[0]) + result.Substring(1);
		}
		return result;
	}

	[GeneratedRegex("[<>\\[\\]\\s&\\(\\):\\.-]", RegexOptions.Compiled)]
	private static partial Regex GetBadCharactersRegex();

	private static string ReplaceBadCharacters(this string str) => GetBadCharactersRegex().Replace(str, "_");
}