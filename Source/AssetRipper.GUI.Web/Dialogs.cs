using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;
using NativeFileDialogs.Net;

namespace AssetRipper.GUI.Web;

internal static class Dialogs
{
	private static readonly object lockObject = new();

	public static class OpenFiles
	{
		public static Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			NfdStatus status = GetUserInput(out string[]? paths);
			//Maybe do something else when user cancels the dialog?
			return Results.Json(paths ?? [], AppJsonSerializerContext.Default.StringArray).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string[]? paths, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			lock (lockObject)
			{
				return Nfd.OpenDialogMultiple(out paths, filters, defaultPath);
			}
		}
	}

	public static class OpenFile
	{
		public static Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			NfdStatus status = GetUserInput(out string? path);
			//Maybe do something else when user cancels the dialog?
			return Results.Json(path ?? "", AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string? path, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			lock (lockObject)
			{
				return Nfd.OpenDialog(out path, filters, defaultPath);
			}
		}
	}

	public static class OpenFolder
	{
		public static Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			NfdStatus status = GetUserInput(out string? path);
			//Maybe do something else when user cancels the dialog?
			return Results.Json(path ?? "", AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string? path, string? defaultPath = null)
		{
			lock (lockObject)
			{
				return Nfd.PickFolder(out path, defaultPath);
			}
		}
	}

	public static class SaveFile
	{
		public static Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			NfdStatus status = GetUserInput(out string? path);
			//Maybe do something else when user cancels the dialog?
			return Results.Json(path ?? "", AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string? path, IDictionary<string, string>? filters = null, string defaultName = "Untitled", string? defaultPath = null)
		{
			lock (lockObject)
			{
				return Nfd.SaveDialog(out path, filters, defaultName, defaultPath);
			}
		}
	}
}
