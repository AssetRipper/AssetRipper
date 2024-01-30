using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class ConfigurationFilesPage : DefaultPage
{
	public static ConfigurationFilesPage Instance { get; } = new();

	public override string? GetTitle() => "Configuration Files";

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		ReadOnlySpan<HtmlTab> tabs = [SingletonsTab.Instance, ListsTab.Instance];
		HtmlTab.WriteNavigation(writer, tabs);
		HtmlTab.WriteContent(writer, tabs);
	}

	public static Task HandleSingletonAddPostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key))
		{
			return Results.BadRequest().ExecuteAsync(context);
		}

		if (!context.Request.Form.TryGetString("Content", out string? content) && Dialogs.OpenFile.TryGetUserInput(out string? path))
		{
			content = File.ReadAllText(path);
		}

		if (content is not null)
		{
			GameFileLoader.Settings.SingletonData[key] = content;
		}

		return Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}

	public static Task HandleSingletonRemovePostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key))
		{
			return Results.BadRequest().ExecuteAsync(context);
		}

		GameFileLoader.Settings.SingletonData[key] = null;

		return Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}

	public static Task HandleListAddPostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key))
		{
			return Results.BadRequest().ExecuteAsync(context);
		}

		if (context.Request.Form.TryGetStringArray("Content", out string?[]? contentArray))
		{
			GameFileLoader.Settings.ListData.GetOrAdd(key).AddRange(contentArray.Where(s => s is not null)!);
		}
		else if (Dialogs.OpenFiles.TryGetUserInput(out string[]? paths))
		{
			List<string> list = GameFileLoader.Settings.ListData.GetOrAdd(key);
			foreach (string path in paths)
			{
				list.Add(File.ReadAllText(path));
			}
		}

		return Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}

	public static Task HandleListRemovePostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key)
			|| GameFileLoader.Settings.ListData[key] is not { } list
			|| !context.Request.Form.TryGetInteger("Index", out int index)
			|| !list.ContainsIndex(index))
		{
			return Results.BadRequest().ExecuteAsync(context);
		}

		list.RemoveAt(index);

		return Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}

	public static Task HandleListReplacePostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key)
			|| GameFileLoader.Settings.ListData[key] is not { } list
			|| !context.Request.Form.TryGetInteger("Index", out int index)
			|| !list.ContainsIndex(index))
		{
			return Results.BadRequest().ExecuteAsync(context);
		}

		if (!context.Request.Form.TryGetString("Content", out string? content) && Dialogs.OpenFile.TryGetUserInput(out string? path))
		{
			content = File.ReadAllText(path);
		}

		if (content is not null)
		{
			list[index] = content;
		}

		return Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}
}
