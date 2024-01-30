using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AssetRipper.GUI.Web.Pages;

internal static class FormCollectionExtensions
{
	public static bool TryGetInteger(this IFormCollection form, string key, out int value)
	{
		if (form.TryGetValue(key, out StringValues values) && int.TryParse(values, out int parsedValue))
		{
			value = parsedValue;
			return true;
		}
		value = default;
		return false;
	}

	public static bool TryGetString(this IFormCollection form, string key, [NotNullWhen(true)] out string? value)
	{
		if (form.TryGetValue(key, out StringValues values))
		{
			value = values;
			return value is not null;
		}
		value = default;
		return false;
	}

	public static bool TryGetStringArray(this IFormCollection form, string key, [NotNullWhen(true)] out string?[]? value)
	{
		if (form.TryGetValue(key, out StringValues values))
		{
			value = values;
			return value is not null;
		}
		value = default;
		return false;
	}
}
