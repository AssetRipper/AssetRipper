using AssetRipper.NativeDialogs;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web;

internal static class Dialogs
{
	public static bool Supported => NativeDialog.Supported;

	public static class OpenFiles
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string[] paths = await OpenFileDialog.OpenFiles() ?? [];
			await Results.Json(paths, AppJsonSerializerContext.Default.StringArray).ExecuteAsync(context);
		}
	}

	public static class OpenFolders
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string[] paths = await OpenFolderDialog.OpenFolders() ?? [];
			await Results.Json(paths, AppJsonSerializerContext.Default.StringArray).ExecuteAsync(context);
		}
	}

	public static class OpenFile
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string path = await OpenFileDialog.OpenFile() ?? "";
			await Results.Json(path, AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}
	}

	public static class OpenFolder
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string path = await OpenFolderDialog.OpenFolder() ?? "";
			await Results.Json(path, AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}
	}

	public static class SaveFile
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string path = await SaveFileDialog.SaveFile() ?? "";
			await Results.Json(path, AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}
	}
}
