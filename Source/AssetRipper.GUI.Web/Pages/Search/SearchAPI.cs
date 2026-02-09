using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Search;

internal static class SearchAPI
{
	public static class Urls
	{
		public const string Base = "/Search";
		public const string View = Base + "/View";
	}

	public const string Query = "q";

	public static string GetViewUrl(string searchQuery)
	{
		return $"{Urls.View}?{Query}={searchQuery.ToUrl()}";
	}

	public static Task GetView(HttpContext context)
	{
		context.Response.DisableCaching();
		
		if (!GameFileLoader.IsLoaded)
		{
			return context.Response.NotFound("No files loaded.");
		}

		string searchQuery = context.Request.Query[Query].ToString();

		return new ViewPage()
		{
			SearchQuery = searchQuery.Trim()
		}.WriteToResponse(context.Response);
	}
}
