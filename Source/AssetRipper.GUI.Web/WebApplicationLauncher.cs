using AssetRipper.GUI.Web.Pages;
using AssetRipper.GUI.Web.Pages.Assets;
using AssetRipper.GUI.Web.Pages.Bundles;
using AssetRipper.GUI.Web.Pages.Collections;
using AssetRipper.GUI.Web.Pages.Resources;
using AssetRipper.GUI.Web.Pages.Scenes;
using AssetRipper.GUI.Web.Pages.Settings;
using AssetRipper.Import.Logging;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.CommandLine;
using System.Diagnostics;

namespace AssetRipper.GUI.Web;

public static class WebApplicationLauncher
{
	private static class Defaults
	{
		public const int Port = 0;
		public const bool LaunchBrowser = true;
	}

	public static void Launch(string[] args)
	{
		RootCommand rootCommand = new() { Description = "AssetRipper" };

		Option<int> portOption = new Option<int>(
			name: "--port",
			description: "If nonzero, the application will attempt to host on this port, instead of finding a random unused port.",
			getDefaultValue: () => Defaults.Port);
		rootCommand.AddOption(portOption);

		Option<bool> launchBrowserOption = new Option<bool>(
			name: "--launch-browser",
			description: "If true, a browser window will be launched automatically.",
			getDefaultValue: () => Defaults.LaunchBrowser);
		rootCommand.AddOption(launchBrowserOption);

		bool shouldRun = false;
		int port = Defaults.Port;
		bool launchBrowser = Defaults.LaunchBrowser;

		rootCommand.SetHandler((int portParsed, bool launchBrowserParsed) =>
		{
			shouldRun = true;
			port = portParsed;
			launchBrowser = launchBrowserParsed;
		}, portOption, launchBrowserOption);

		rootCommand.Invoke(args);

		if (shouldRun)
		{
			Launch(port, launchBrowser);
		}
	}

	public static void Launch(int port = Defaults.Port, bool launchBrowser = Defaults.LaunchBrowser)
	{
		WelcomeMessage.Print();

		Logger.Add(new FileLogger());
		Logger.LogSystemInformation("AssetRipper");
		Logger.Add(new ConsoleLogger());

		WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions()
		{
#if DEBUG
			EnvironmentName = Environments.Development,
#else
			EnvironmentName = Environments.Production,
#endif
		});

		builder.WebHost.UseUrls($"http://127.0.0.1:{port}");

		builder.Services.AddTransient<ErrorHandlingMiddleware>(static (_) => new());
		builder.Services.ConfigureHttpJsonOptions(options =>
		{
			options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
		});

		builder.Logging.ConfigureLoggingLevel();

		WebApplication app = builder.Build();

		// Configure the HTTP request pipeline.
#if !DEBUG
		app.UseMiddleware<ErrorHandlingMiddleware>();
#endif
		if (launchBrowser)
		{
			app.Lifetime.ApplicationStarted.Register(() =>
			{
				string? address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
				if (address is not null)
				{
					OpenUrl(address);
				}
			});
		}

		//Static files
		app.MapStaticFile("/favicon.ico", "image/x-icon");
		app.MapStaticFile("/css/site.css", "text/css");
		app.MapStaticFile("/js/site.js", "text/javascript");
		app.MapStaticFile("/js/commands_page.js", "text/javascript");

		//Normal Pages
		app.MapGet("/", (context) =>
		{
			context.Response.DisableCaching();
			return IndexPage.Instance.WriteToResponse(context.Response);
		});
		app.MapGet("/Commands", CommandsPage.Instance.ToResult);
		app.MapGet("/Privacy", PrivacyPage.Instance.ToResult);
		app.MapGet("/Licenses", LicensesPage.Instance.ToResult);

		app.MapGet("/ConfigurationFiles", (context) =>
		{
			context.Response.DisableCaching();
			return ConfigurationFilesPage.Instance.WriteToResponse(context.Response);
		});
		app.MapPost("/ConfigurationFiles/Singleton/Add", ConfigurationFilesPage.HandleSingletonAddPostRequest);
		app.MapPost("/ConfigurationFiles/Singleton/Remove", ConfigurationFilesPage.HandleSingletonRemovePostRequest);
		app.MapPost("/ConfigurationFiles/List/Add", ConfigurationFilesPage.HandleListAddPostRequest);
		app.MapPost("/ConfigurationFiles/List/Remove", ConfigurationFilesPage.HandleListRemovePostRequest);
		app.MapPost("/ConfigurationFiles/List/Replace", ConfigurationFilesPage.HandleListReplacePostRequest);

		app.MapGet("/Settings/Edit", (context) =>
		{
			context.Response.DisableCaching();
			return SettingsPage.Instance.WriteToResponse(context.Response);
		});
		app.MapPost("/Settings/Update", SettingsPage.HandlePostRequest);

		//Assets
		app.MapGet(AssetAPI.Urls.View, AssetAPI.GetView);
		app.MapGet(AssetAPI.Urls.Image, AssetAPI.GetImageData);
		app.MapGet(AssetAPI.Urls.Audio, AssetAPI.GetAudioData);
		app.MapGet(AssetAPI.Urls.Model, AssetAPI.GetModelData);
		app.MapGet(AssetAPI.Urls.Font, AssetAPI.GetFontData);
		app.MapGet(AssetAPI.Urls.Json, AssetAPI.GetJson);
		app.MapGet(AssetAPI.Urls.Yaml, AssetAPI.GetYaml);
		app.MapGet(AssetAPI.Urls.Text, AssetAPI.GetText);
		app.MapGet(AssetAPI.Urls.Binary, AssetAPI.GetBinaryData);

		//Bundles
		app.MapGet(BundleAPI.Urls.View, BundleAPI.GetView);

		//Collections
		app.MapGet(CollectionAPI.Urls.View, CollectionAPI.GetView);
		app.MapGet(CollectionAPI.Urls.Count, CollectionAPI.GetCount);

		//Resources
		app.MapGet(ResourceAPI.Urls.View, ResourceAPI.GetView);
		app.MapGet(ResourceAPI.Urls.Data, ResourceAPI.GetData);

		//Scenes
		app.MapGet(SceneAPI.Urls.View, SceneAPI.GetView);

		app.MapPost("/Localization", (context) =>
		{
			context.Response.DisableCaching();
			if (context.Request.Query.TryGetValue("code", out StringValues code))
			{
				string? language = code;
				if (language is not null && LocalizationLoader.LanguageNameDictionary.ContainsKey(language))
				{
					Localization.LoadLanguage(language);
				}
			}
			return Results.Redirect("/").ExecuteAsync(context);
		});

		//Commands
		app.MapPost("/Export", Commands.HandleCommand<Commands.Export>);
		app.MapPost("/LoadFile", Commands.HandleCommand<Commands.LoadFile>);
		app.MapPost("/LoadFolder", Commands.HandleCommand<Commands.LoadFolder>);
		app.MapPost("/Reset", Commands.HandleCommand<Commands.Reset>);

		//Dialogs
		app.MapGet("/Dialogs/SaveFile", Dialogs.SaveFile.HandleGetRequest);
		app.MapGet("/Dialogs/OpenFolder", Dialogs.OpenFolder.HandleGetRequest);
		app.MapGet("/Dialogs/OpenFile", Dialogs.OpenFile.HandleGetRequest);
		app.MapGet("/Dialogs/OpenFiles", Dialogs.OpenFiles.HandleGetRequest);

		//File API
		app.MapGet("/IO/File/Exists", (context) =>
		{
			context.Response.DisableCaching();
			if (context.Request.Query.TryGetValue("Path", out StringValues path))
			{
				bool exists = File.Exists(path);
				return Results.Json(exists, AppJsonSerializerContext.Default.Boolean).ExecuteAsync(context);
			}
			else
			{
				return Results.BadRequest().ExecuteAsync(context);
			}
		});
		app.MapGet("/IO/Directory/Exists", (context) =>
		{
			context.Response.DisableCaching();
			if (context.Request.Query.TryGetValue("Path", out StringValues path))
			{
				bool exists = Directory.Exists(path);
				return Results.Json(exists, AppJsonSerializerContext.Default.Boolean).ExecuteAsync(context);
			}
			else
			{
				return Results.BadRequest().ExecuteAsync(context);
			}
		});
		app.MapGet("/IO/Directory/Empty", (context) =>
		{
			context.Response.DisableCaching();
			if (context.Request.Query.TryGetValue("Path", out StringValues stringValues))
			{
				string? path = stringValues;
				bool empty = !Directory.Exists(path) || !Directory.EnumerateFileSystemEntries(path).Any();
				return Results.Json(empty, AppJsonSerializerContext.Default.Boolean).ExecuteAsync(context);
			}
			else
			{
				return Results.BadRequest().ExecuteAsync(context);
			}
		});

		app.Run();
	}

	private static ILoggingBuilder ConfigureLoggingLevel(this ILoggingBuilder builder)
	{
		builder.Services.Add(ServiceDescriptor.Singleton<IConfigureOptions<LoggerFilterOptions>>(
			new LifetimeOrWarnConfigureOptions()));
		return builder;
	}

	private sealed class LifetimeOrWarnConfigureOptions : ConfigureOptions<LoggerFilterOptions>
	{
		public LifetimeOrWarnConfigureOptions() : base(AddRule)
		{
		}

		private static void AddRule(LoggerFilterOptions options)
		{
			options.Rules.Add(new LoggerFilterRule(null, null, LogLevel.Information, static (provider, category, logLevel) =>
			{
				return category is "Microsoft.Hosting.Lifetime" || logLevel >= LogLevel.Warning;
			}));
		}
	}

	private static void OpenUrl(string url)
	{
		try
		{
			if (OperatingSystem.IsWindows())
			{
				Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
			}
			else if (OperatingSystem.IsLinux())
			{
				Process.Start("xdg-open", url);
			}
			else if (OperatingSystem.IsMacOS())
			{
				Process.Start("open", url);
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to launch web browser for: {url}", ex);
		}
	}

	private static void MapGet(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, Func<IResult> handler)
	{
		endpoints.MapGet(pattern, (context) =>
		{
			IResult result = handler.Invoke();
			return result.ExecuteAsync(context);
		});
	}

	private static void MapGet(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, Func<Task<IResult>> handler)
	{
		endpoints.MapGet(pattern, async (context) =>
		{
			IResult result = await handler.Invoke();
			await result.ExecuteAsync(context);
		});
	}

	private static void MapStaticFile(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string path, string contentType)
	{
		endpoints.MapGet(path, async () =>
		{
			byte[] data = await StaticContentLoader.Load(path);
			return Results.Bytes(data, contentType);
		});
	}
}
