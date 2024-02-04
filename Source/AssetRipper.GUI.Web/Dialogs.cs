using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;
using NativeFileDialogs.Net;
using System.Runtime.InteropServices;

namespace AssetRipper.GUI.Web;

internal static class Dialogs
{
	private static readonly object lockObject = new();

	public static bool Supported
	{
		get
		{
			return !OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture is Architecture.X64;
		}
	}

	private static void ThrowIfNotSupported()
	{
		if (!Supported)
		{
			throw new PlatformNotSupportedException("NativeFileDialogs is not supported on this platform");
		}
	}

	public static class OpenFiles
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string[]? paths = await Task.Run(static () =>
			{
				if (Supported)
				{
					GetUserInput(out string[]? paths);
					return paths;
				}
				else
				{
					return null;
				}
			});
			await Results.Json(paths ?? [], AppJsonSerializerContext.Default.StringArray).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string[]? paths, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			ThrowIfNotSupported();
			lock (lockObject)
			{
				return Nfd.OpenDialogMultiple(out paths, filters, defaultPath);
			}
		}

		public static bool TryGetUserInput([NotNullWhen(true)] out string[]? paths, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			NfdStatus status = GetUserInput(out paths, filters, defaultPath);
			return status == NfdStatus.Ok && paths is not null;
		}
	}

	public static class OpenFile
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string? path = await Task.Run(static () =>
			{
				if (Supported)
				{
					GetUserInput(out string? path);
					return path;
				}
				else
				{
					return null;
				}
			});
			await Results.Json(path ?? "", AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string? path, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			ThrowIfNotSupported();
			lock (lockObject)
			{
				return Nfd.OpenDialog(out path, filters, defaultPath);
			}
		}

		public static bool TryGetUserInput([NotNullWhen(true)] out string? path, IDictionary<string, string>? filters = null, string? defaultPath = null)
		{
			NfdStatus status = GetUserInput(out path, filters, defaultPath);
			return status == NfdStatus.Ok && path is not null;
		}
	}

	public static class OpenFolder
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string? path = await Task.Run(static () =>
			{
				if (Supported)
				{
					GetUserInput(out string? path);
					return path;
				}
				else
				{
					return null;
				}
			});
			await Results.Json(path ?? "", AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string? path, string? defaultPath = null)
		{
			ThrowIfNotSupported();
			lock (lockObject)
			{
				return Nfd.PickFolder(out path, defaultPath);
			}
		}
	}

	public static class SaveFile
	{
		public static async Task HandleGetRequest(HttpContext context)
		{
			context.Response.DisableCaching();
			string? path = await Task.Run(static () =>
			{
				if (Supported)
				{
					GetUserInput(out string? path);
					return path;
				}
				else
				{
					return null;
				}
			});
			await Results.Json(path ?? "", AppJsonSerializerContext.Default.String).ExecuteAsync(context);
		}

		public static NfdStatus GetUserInput(out string? path, IDictionary<string, string>? filters = null, string defaultName = "Untitled", string? defaultPath = null)
		{
			ThrowIfNotSupported();
			lock (lockObject)
			{
				return Nfd.SaveDialog(out path, filters, defaultName, defaultPath);
			}
		}
	}
}
