using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AssetRipper.GUI.Web.Pages;

public static class Commands
{
	public readonly struct LoadFile : ICommand
	{
		static async Task ICommand.Start(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string[]? paths;
			if (form.TryGetValue("Path", out StringValues values))
			{
				paths = values;
			}
			else
			{
				Dialogs.OpenFiles.GetUserInput(out paths);
			}

			if (paths is { Length: > 0 })
			{
				GameFileLoader.LoadAndProcess(paths);
			}
		}
	}

	public readonly struct LoadFolder : ICommand
	{
		static async Task ICommand.Start(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string? path;
			if (form.TryGetValue("Path", out StringValues values))
			{
				path = values;
			}
			else
			{
				Dialogs.OpenFolder.GetUserInput(out path);
			}

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.LoadAndProcess([path]);
			}
		}
	}

	public readonly struct Export : ICommand
	{
		static async Task ICommand.Start(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string? path;
			if (form.TryGetValue("Path", out StringValues values))
			{
				path = values;
			}
			else
			{
				Dialogs.OpenFolder.GetUserInput(out path);
			}

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.Export(path);
			}
		}
	}

	public readonly struct Reset : ICommand
	{
		static Task ICommand.Start(HttpRequest request)
		{
			GameFileLoader.Reset();
			return Task.CompletedTask;
		}
	}

	public static Task HandleCommand<T>(HttpContext context) where T : ICommand
	{
		context.Response.Redirect(T.RedirectionTarget);
		return T.Start(context.Request);
	}
}
