using AssetRipper.Import.Configuration;
using AssetRipper.NativeDialogs;
using AssetRipper.SourceGenerated.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class ConfigurationFilesPage : DefaultPage
{
	public static ConfigurationFilesPage Instance { get; } = new();

	public override string? GetTitle() => Localization.ConfigurationFiles;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		ReadOnlySpan<HtmlTab> tabs = [SingletonsTab.Instance, ListsTab.Instance];
		HtmlTab.WriteNavigation(writer, tabs);
		HtmlTab.WriteContent(writer, tabs);
	}

	public static async Task HandleSingletonAddPostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key))
		{
			await Results.BadRequest().ExecuteAsync(context);
			return;
		}

		if (!context.Request.Form.TryGetString("Content", out string? content))
		{
			string? path = await OpenFileDialog.OpenFile();
			if (!string.IsNullOrEmpty(path))
			{
				content = File.ReadAllText(path);
			}
		}

		if (content is not null)
		{
			GameFileLoader.Settings.SingletonData.GetOrAdd(key).Text = content;
		}

		await Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}

	public static Task HandleSingletonRemovePostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key))
		{
			return Results.BadRequest().ExecuteAsync(context);
		}

		GameFileLoader.Settings.SingletonData[key]?.Clear();

		return Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}

	public static async Task HandleListAddPostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key))
		{
			await Results.BadRequest().ExecuteAsync(context);
			return;
		}

		if (context.Request.Form.TryGetStringArray("Content", out string?[]? contentArray))
		{
			DataSet set = GameFileLoader.Settings.ListData.GetOrAdd(key);
			set.Strings.AddRange(contentArray.WhereNotNull());
		}
		else
		{
			string[]? paths = await OpenFileDialog.OpenFiles();
			if (paths is { Length: > 0 })
			{
				DataSet set = GameFileLoader.Settings.ListData.GetOrAdd(key);
				foreach (string path in paths)
				{
					set.Strings.Add(File.ReadAllText(path));
				}
			}
		}

		await Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
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

	public static async Task HandleListReplacePostRequest(HttpContext context)
	{
		if (!context.Request.Form.TryGetString("Key", out string? key)
			|| GameFileLoader.Settings.ListData[key] is not { } list
			|| !context.Request.Form.TryGetInteger("Index", out int index)
			|| !list.ContainsIndex(index))
		{
			await Results.BadRequest().ExecuteAsync(context);
			return;
		}

		if (!context.Request.Form.TryGetString("Content", out string? content))
		{
			string? path = await OpenFileDialog.OpenFile();
			if (!string.IsNullOrEmpty(path))
			{
				content = File.ReadAllText(path);
			}
		}

		if (content is not null)
		{
			DataSet.StringAccessor strings = list.Strings;
			strings[index] = content;
		}

		await Results.Redirect("/ConfigurationFiles").ExecuteAsync(context);
	}
}
