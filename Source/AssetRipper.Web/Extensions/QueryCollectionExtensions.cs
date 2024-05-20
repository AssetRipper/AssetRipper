using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AssetRipper.Web.Extensions;

public static class QueryCollectionExtensions
{
	public static bool TryGetValue(this IQueryCollection query, string key, [NotNullWhen(true)] out string? value)
	{
		if (query.TryGetValue(key, out StringValues values))
		{
			value = values.ToString();
			return true;
		}
		else
		{
			value = null;
			return false;
		}
	}
}
