using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AssetRipper.IO.Files;

public partial class FileSystem
{
	public static bool ContainsUnsafeUnicode(string value)
	{
		ReadOnlySpan<char> remaining = value;
		while (!remaining.IsEmpty)
		{
			OperationStatus status = Rune.DecodeFromUtf16(remaining, out Rune rune, out int charsConsumed);
			if (status != OperationStatus.Done)
			{
				return true;
			}

			UnicodeCategory category = Rune.GetUnicodeCategory(rune);
			if (category is UnicodeCategory.Control
				or UnicodeCategory.Format
				or UnicodeCategory.PrivateUse
				or UnicodeCategory.OtherNotAssigned
				or UnicodeCategory.Surrogate
				|| rune.Value == 0xFFFD)
			{
				return true;
			}

			remaining = remaining[charsConsumed..];
		}

		return false;
	}

	public static string GetSafeFileName(string value, string? fallback = null)
	{
		fallback = GetSafeFallback(fallback, value);
		if (ContainsUnsafeUnicode(value))
		{
			return fallback;
		}

		string result = FixInvalidFileNameCharacters(value.Normalize(NormalizationForm.FormC))
			.Trim()
			.TrimEnd(' ', '.');
		return string.IsNullOrEmpty(result) || result is "." or ".." || IsReservedName(result)
			? fallback
			: result;
	}

	public static string GetSafeRelativePath(string path)
	{
		List<string> safeSegments = [];
		foreach (string segment in path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries))
		{
			if (segment == ".")
			{
				continue;
			}

			safeSegments.Add(GetSafeFileName(segment));
		}

		return safeSegments.Count == 0
			? "Assets"
			: string.Join(System.IO.Path.DirectorySeparatorChar, safeSegments);
	}

	public static string EscapeUnsafeUnicode(string value)
	{
		StringBuilder builder = new(value.Length);
		ReadOnlySpan<char> remaining = value;
		while (!remaining.IsEmpty)
		{
			OperationStatus status = Rune.DecodeFromUtf16(remaining, out Rune rune, out int charsConsumed);
			if (status != OperationStatus.Done)
			{
				builder.Append($"\\u{(int)remaining[0]:X4}");
				remaining = remaining[1..];
				continue;
			}

			UnicodeCategory category = Rune.GetUnicodeCategory(rune);
			if (category is UnicodeCategory.Control
				or UnicodeCategory.Format
				or UnicodeCategory.PrivateUse
				or UnicodeCategory.OtherNotAssigned
				or UnicodeCategory.Surrogate
				or UnicodeCategory.NonSpacingMark
				or UnicodeCategory.SpacingCombiningMark
				or UnicodeCategory.EnclosingMark
				|| rune.Value == 0xFFFD)
			{
				builder.Append(rune.Value <= ushort.MaxValue
					? $"\\u{rune.Value:X4}"
					: $"\\U{rune.Value:X8}");
			}
			else if (rune.Value == '\\')
			{
				builder.Append("\\\\");
			}
			else
			{
				builder.Append(rune.ToString());
			}

			remaining = remaining[charsConsumed..];
		}

		return builder.ToString();
	}

	private static string GetSafeFallback(string? fallback, string originalValue)
	{
		if (string.IsNullOrWhiteSpace(fallback) || ContainsUnsafeUnicode(fallback))
		{
			fallback = $"Recovered_{GetStableNameHash(originalValue)}";
		}

		string result = FixInvalidFileNameCharacters(fallback).Trim().TrimEnd(' ', '.');
		return string.IsNullOrEmpty(result) || result is "." or ".." || IsReservedName(result)
			? $"Recovered_{GetStableNameHash(originalValue)}"
			: result;
	}

	private static string GetStableNameHash(string value)
	{
		byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
		return Convert.ToHexString(hash.AsSpan(0, 4));
	}
}
