using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Photino.NET;

namespace AssetRipper.GUI.Web;

internal static class Dialogs
{
	private static readonly object lockObject = new();

	private static PhotinoWindow? Window => WebApplicationLauncher.PhotinoWindow;

	[MemberNotNullWhen(true, nameof(Window))]
	public static bool Supported => Window != null;

	public static class OpenFiles
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string[] paths = Supported
				? await Window.ShowOpenFileAsync(Localization.SelectFiles, null, true)
				: [];
			await Results.Json(paths, AppJsonSerializerContext.Default.StringArray).ExecuteAsync(context);
		}

		public static string[]? GetUserInput(IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			return TryGetUserInput(out string[]? paths, filters, defaultPath) ? paths : null;
		}

		public static bool TryGetUserInput([NotNullWhen(true)] out string[]? paths, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			lock (lockObject)
			{
				paths = Window?.ShowOpenFile(Localization.SelectFiles, defaultPath, true, filters?.Select(pair => (pair.Key, new string[] { pair.Value })).ToArray());
			}
			return paths is { Length: > 0 };
		}
	}

	public static class OpenFolders
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string[] paths = Supported
				? await Window.ShowOpenFolderAsync(Localization.SelectFolders, null, true)
				: [];
			await Results.Json(paths, AppJsonSerializerContext.Default.StringArray).ExecuteAsync(context);
		}

		public static string[]? GetUserInput(string? defaultPath = null)
		{
			return TryGetUserInput(out string[]? paths, defaultPath) ? paths : null;
		}

		public static bool TryGetUserInput([NotNullWhen(true)] out string[]? paths, string? defaultPath = null)
		{
			lock (lockObject)
			{
				paths = Window?.ShowOpenFolder(Localization.SelectFolders, defaultPath, true);
			}
			return paths is { Length: > 0 };
		}
	}

	public static class OpenFile
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string[] paths = Supported ? await Window.ShowOpenFileAsync(Localization.SelectFile) : [];
			string path = paths.Length is 1 ? paths[0] : "";
			await Results.Json(path, AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static string? GetUserInput(out string? path, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			return TryGetUserInput(out path, filters, defaultPath) ? path : null;
		}

		public static bool TryGetUserInput([NotNullWhen(true)] out string? path, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			string[] paths;
			lock (lockObject)
			{
				paths = Supported
					? Window.ShowOpenFile(Localization.SelectFile, defaultPath, false, filters?.Select(pair => (pair.Key, new string[] { pair.Value })).ToArray())
					: [];
			}
			path = paths.Length is 1 ? paths[0] : null;
			return paths is { Length: > 0 };
		}
	}

	public static class OpenFolder
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string[] paths = Supported ? await Window.ShowOpenFolderAsync(Localization.SelectFolder) : [];
			string path = paths.Length is 1 ? paths[0] : "";
			await Results.Json(path, AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static string? GetUserInput(string? defaultPath = null)
		{
			string[]? paths = Window?.ShowOpenFolder(Localization.SelectFolder, defaultPath);
			return paths is { Length: 1 } ? paths[0] : null;
		}
	}

	public static class SaveFile
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string? path = Supported ? await Window.ShowSaveFileAsync(Localization.Save) : null;
			await Results.Json(path ?? "", AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}
	}
}
