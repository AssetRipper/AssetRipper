using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages;

public static class Commands
{
	public readonly struct Load : ICommand
	{
		static Task ICommand.Start(HttpRequest request)
		{
			string? path = request.Form["Path"];

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.LoadAndProcess([path]);
			}
			return Task.CompletedTask;
		}
	}

	public readonly struct Export : ICommand
	{
		static Task ICommand.Start(HttpRequest request)
		{
			string? path = request.Form["Path"];

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.Export(path);
			}
			return Task.CompletedTask;
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
