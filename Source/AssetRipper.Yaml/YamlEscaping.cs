using System.Numerics;
using System.Text;

namespace AssetRipper.Yaml;

internal static class YamlEscaping
{
	//https://yaml.org/spec/1.1/current.html#escaping%20in%20double-quoted%20style/

	[return: NotNullIfNotNull(nameof(value))]
	public static string? Escape(string? value)
	{
		if (value is null)
		{
			return null;
		}

		if (value.Length == 0)
		{
			return "";
		}

		int index = IndexOfFirstCharacterToEscape(value);
		if (index < 0)
		{
			return value;
		}

		ReadOnlySpan<char> span = value;

		StringBuilder sb = new((int)BitOperations.RoundUpToPowerOf2((uint)value.Length));
		sb.Append(span[..index]);
		foreach (char c in span[index..])
		{
			WriteCharacter(sb, c);
		}
		return sb.ToString();
	}

	public static bool TryEscape(char c, [NotNullWhen(true)] out string? escapeSequence)
	{
		if (NeedsEscaped(c))
		{
			escapeSequence = c switch
			{
				'\\' => "\\\\",
				'"' => "\\\"",
				'\n' => "\\n",
				'\r' => "\\r",
				'\t' => "\\t",
				_ => EscapeAsHex(c),
			};
			return true;
		}
		else
		{
			escapeSequence = null;
			return false;
		}
	}

	private static void WriteCharacter(StringBuilder sb, char c)
	{
		if (NeedsEscaped(c))
		{
			switch (c)
			{
				case '\\':
					sb.Append('\\').Append('\\');
					break;
				case '"':
					sb.Append('\\').Append('"');
					break;
				case '\n':
					sb.Append('\\').Append('n');
					break;
				case '\r':
					sb.Append('\\').Append('r');
					break;
				case '\t':
					sb.Append('\\').Append('t');
					break;
				default:
					WriteEscapedAsHex(sb, c);
					break;
			}
		}
		else
		{
			sb.Append(c);
		}
	}

	public static int IndexOfFirstCharacterToEscape(ReadOnlySpan<char> span)
	{
		for (int i = 0; i < span.Length; i++)
		{
			if (NeedsEscaped(span[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private static bool NeedsEscaped(char c)
	{
		//A large portion of Unicode does not need escaping, but it's simpler to escape all non-ascii characters.
		//https://en.wikipedia.org/wiki/ASCII
		return c is not (>= (char)0x20 and <= (char)0x7E) or '"' or '\\';
	}

	private static string EscapeAsHex(char c)
	{
		const string HexCharacters = "0123456789ABCDEF";
		ushort value = c;

		if (value > byte.MaxValue)
		{
			//Format as \uXXXX
			return $"\\u{HexCharacters[(value & 0xF000) >> 12]}{HexCharacters[(value & 0xF00) >> 8]}{HexCharacters[(value & 0xF0) >> 4]}{HexCharacters[value & 0xF]}";
		}
		else
		{
			//Format as \xXX
			return $"\\x{HexCharacters[(value & 0xF0) >> 4]}{HexCharacters[value & 0xF]}";
		}
	}

	private static void WriteEscapedAsHex(StringBuilder sb, char c)
	{
		const string HexCharacters = "0123456789ABCDEF";
		ushort value = c;

		if (value > byte.MaxValue)
		{
			//Format as \uXXXX
			ReadOnlySpan<char> span =
				[
					'\\',
					'u',
					HexCharacters[(value & 0xF000) >> 12],
					HexCharacters[(value & 0xF00) >> 8],
					HexCharacters[(value & 0xF0) >> 4],
					HexCharacters[value & 0xF],
				];
			sb.Append(span);
		}
		else
		{
			//Format as \xXX
			ReadOnlySpan<char> span =
				[
					'\\',
					'x',
					HexCharacters[(value & 0xF0) >> 4],
					HexCharacters[value & 0xF],
				];
			sb.Append(span);
		}
	}
}
