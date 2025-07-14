using AssetRipper.GUI.Web.Documentation;
using AssetRipper.GUI.Web.Pages;
using AssetRipper.GUI.Web.Pages.Assets;
using AssetRipper.GUI.Web.Pages.Bundles;
using AssetRipper.GUI.Web.Pages.Collections;
using AssetRipper.GUI.Web.Pages.FailedFiles;
using AssetRipper.GUI.Web.Pages.Resources;
using AssetRipper.GUI.Web.Pages.Scenes;
using AssetRipper.GUI.Web.Pages.Settings;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Utils;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SwaggerThemes;
using System.Diagnostics;

namespace AssetRipper.GUI.Web;

public static class WebApplicationLauncher
{
	internal static class Defaults
	{
		public const int Port = 0;
		public const bool LaunchBrowser = true;
		public const bool Log = true;
		public const string? LogPath = null;
	}

	public static void Launch(string[] args)
	{
		Arguments? arguments = Arguments.Parse(args);

		if (arguments is null)
		{
			return;
		}

		foreach (string localWebFile in arguments.LocalWebFiles ?? [])
		{
			if (File.Exists(localWebFile))
			{
				string fileName = Path.GetFileName(localWebFile);
				string webPrefix = Path.GetExtension(fileName) switch
				{
					".css" => "/css/",
					".js" => "/js/",
					_ => "/"
				};
				StaticContentLoader.Add(webPrefix + fileName, File.ReadAllBytes(localWebFile));
			}
			else
			{
				Console.WriteLine($"File '{localWebFile}' does not exist.");
			}
		}

		Launch(arguments.Port, arguments.LaunchBrowser, arguments.Log, arguments.LogPath);
	}

	public static void Launch(int port = Defaults.Port, bool launchBrowser = Defaults.LaunchBrowser, bool log = Defaults.Log, string? logPath = Defaults.LogPath)
	{
		WelcomeMessage.Print();

		if (log)
		{
			if (string.IsNullOrEmpty(logPath))
			{
				logPath = ExecutingDirectory.Combine($"AssetRipper_{DateTime.Now:yyyyMMdd_HHmmss}.log");
				RotateLogs(logPath);
			}
			Logger.Add(new FileLogger(logPath));
		}
		Logger.LogSystemInformation("AssetRipper");
		Logger.Add(new ConsoleLogger());

		Localization.LoadLanguage(GameFileLoader.Settings.LanguageCode);

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
			options.SerializerOptions.TypeInfoResolverChain.Insert(1, PathSerializerContext.Default);
			options.SerializerOptions.TypeInfoResolverChain.Insert(2, NullSerializerContext.Instance);
		});

		builder.Services.AddOpenApi(options =>
		{
			options.AddOperationTransformer(new ClearOperationTagsTransformer());
			options.AddOperationTransformer(new InsertionOperationTransformer());
			options.AddDocumentTransformer(new ClearDocumentTagsTransformer());
			options.AddDocumentTransformer(new SortDocumentPathsTransformer());
		});

		builder.Services.AddEndpointsApiExplorer();

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

		app.MapOpenApi(DocumentationPaths.OpenApi);
		app.UseSwaggerUI(Theme.Gruvbox, null, c =>
		{
			// Point to the static OpenAPI file
			c.SwaggerEndpoint(DocumentationPaths.OpenApi, "AssetRipper API");
		});

		//Static files
		app.MapStaticFile("/favicon.ico", "image/x-icon");
		app.MapStaticFile("/css/site.css", "text/css");
		app.MapStaticFile("/js/site.js", "text/javascript");
		app.MapStaticFile("/js/commands_page.js", "text/javascript");
		app.MapStaticFile("/js/mesh_preview.js", "text/javascript");
		OnlineDependencies.MapDependencies(app);

		//Normal Pages
		app.MapGet("/", (context) =>
		{
			context.Response.DisableCaching();
			return IndexPage.Instance.WriteToResponse(context.Response);
		})
			.WithSummary("The home page")
			.ProducesHtmlPage();
		app.MapGet("/Commands", CommandsPage.Instance.ToResult).ProducesHtmlPage();
		app.MapGet("/Privacy", PrivacyPage.Instance.ToResult).ProducesHtmlPage();
		app.MapGet("/Licenses", LicensesPage.Instance.ToResult).ProducesHtmlPage();

		app.MapGet("/ConfigurationFiles", (context) =>
		{
			context.Response.DisableCaching();
			return ConfigurationFilesPage.Instance.WriteToResponse(context.Response);
		}).ProducesHtmlPage();
		app.MapPost("/ConfigurationFiles/Singleton/Add", ConfigurationFilesPage.HandleSingletonAddPostRequest);
		app.MapPost("/ConfigurationFiles/Singleton/Remove", ConfigurationFilesPage.HandleSingletonRemovePostRequest);
		app.MapPost("/ConfigurationFiles/List/Add", ConfigurationFilesPage.HandleListAddPostRequest);
		app.MapPost("/ConfigurationFiles/List/Remove", ConfigurationFilesPage.HandleListRemovePostRequest);
		app.MapPost("/ConfigurationFiles/List/Replace", ConfigurationFilesPage.HandleListReplacePostRequest);

		app.MapGet("/Settings/Edit", (context) =>
		{
			context.Response.DisableCaching();
			return SettingsPage.Instance.WriteToResponse(context.Response);
		}).ProducesHtmlPage();
		app.MapPost("/Settings/Update", SettingsPage.HandlePostRequest);

		//Assets
		app.MapGet(AssetAPI.Urls.View, AssetAPI.GetView).ProducesHtmlPage();
		app.MapGet(AssetAPI.Urls.Image, AssetAPI.GetImageData)
			.Produces<byte[]>(contentType: "application/octet-stream")
			.WithAssetPathParameter()
			.WithImageExtensionParameter();
		app.MapGet(AssetAPI.Urls.Audio, AssetAPI.GetAudioData)
			.Produces<byte[]>(contentType: "application/octet-stream")
			.WithAssetPathParameter();
		app.MapGet(AssetAPI.Urls.Model, AssetAPI.GetModelData)
			.Produces<byte[]>(contentType: "application/octet-stream")
			.WithAssetPathParameter();
		app.MapGet(AssetAPI.Urls.Font, AssetAPI.GetFontData)
			.Produces<byte[]>(contentType: "application/octet-stream")
			.WithAssetPathParameter();
		app.MapGet(AssetAPI.Urls.Video, AssetAPI.GetVideoData)
			.Produces<byte[]>(contentType: "application/octet-stream")
			.WithAssetPathParameter();
		app.MapGet(AssetAPI.Urls.Json, AssetAPI.GetJson)
			.Produces<string>(contentType: "application/json")
			.WithAssetPathParameter();
		app.MapGet(AssetAPI.Urls.Yaml, AssetAPI.GetYaml)
			.Produces<string>(contentType: "text/yaml")
			.WithAssetPathParameter();
		app.MapGet(AssetAPI.Urls.Text, AssetAPI.GetText)
			.Produces<string>(contentType: "text/plain")
			.WithAssetPathParameter();
		app.MapGet(AssetAPI.Urls.Binary, AssetAPI.GetBinaryData)
			.Produces<byte[]>(contentType: "application/octet-stream")
			.WithAssetPathParameter();

		//Bundles
		app.MapGet(BundleAPI.Urls.View, BundleAPI.GetView).ProducesHtmlPage();

		//Collections
		app.MapGet(CollectionAPI.Urls.View, CollectionAPI.GetView).ProducesHtmlPage();
		app.MapGet(CollectionAPI.Urls.Count, CollectionAPI.GetCount)
			.WithSummary("Get the number of elements in the collection.")
			.Produces<int>();

		//Failed Files
		app.MapGet(FailedFileAPI.Urls.View, FailedFileAPI.GetView).ProducesHtmlPage();
		app.MapGet(FailedFileAPI.Urls.StackTrace, FailedFileAPI.GetStackTrace)
			.Produces<string>();

		//Resources
		app.MapGet(ResourceAPI.Urls.View, ResourceAPI.GetView).ProducesHtmlPage();
		app.MapGet(ResourceAPI.Urls.Data, ResourceAPI.GetData)
			.Produces<byte[]>(contentType: "application/octet-stream");

		//Scenes
		app.MapGet(SceneAPI.Urls.View, SceneAPI.GetView).ProducesHtmlPage();

		app.MapPost("/Localization", (context) =>
		{
			context.Response.DisableCaching();
			if (context.Request.Query.TryGetValue("code", out StringValues code))
			{
				string? language = code;
				Localization.LoadLanguage(language);
				GameFileLoader.Settings.LanguageCode = Localization.CurrentLanguageCode;
				GameFileLoader.Settings.MaybeSaveToDefaultPath();
			}
			return Results.Redirect("/").ExecuteAsync(context);
		})
			.WithQueryStringParameter("Code", "Language code", true)
			.Produces(StatusCodes.Status302Found);

		//Commands
		app.MapPost("/Export/UnityProject", Commands.HandleCommand<Commands.ExportUnityProject>)
			.AcceptsFormDataContainingPath()
			.Produces(StatusCodes.Status302Found);
		app.MapPost("/Export/PrimaryContent", Commands.HandleCommand<Commands.ExportPrimaryContent>)
			.AcceptsFormDataContainingPath()
			.Produces(StatusCodes.Status302Found);
		app.MapPost("/LoadFile", Commands.HandleCommand<Commands.LoadFile>)
			.AcceptsFormDataContainingPath()
			.Produces(StatusCodes.Status302Found);
		app.MapPost("/LoadFolder", Commands.HandleCommand<Commands.LoadFolder>)
			.AcceptsFormDataContainingPath()
			.Produces(StatusCodes.Status302Found);
		app.MapPost("/Reset", Commands.HandleCommand<Commands.Reset>);

		//Dialogs
		app.MapGet("/Dialogs/SaveFile", Dialogs.SaveFile.HandleGetRequest).Produces<string>();
		app.MapGet("/Dialogs/OpenFolder", Dialogs.OpenFolder.HandleGetRequest).Produces<string>();
		app.MapGet("/Dialogs/OpenFolders", Dialogs.OpenFolders.HandleGetRequest).Produces<string>();
		app.MapGet("/Dialogs/OpenFile", Dialogs.OpenFile.HandleGetRequest).Produces<string>();
		app.MapGet("/Dialogs/OpenFiles", Dialogs.OpenFiles.HandleGetRequest).Produces<string>();

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
		})
			.Produces<bool>()
			.WithQueryStringParameter("Path", required: true);

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
		})
			.Produces<bool>()
			.WithQueryStringParameter("Path", required: true);

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
		})
			.Produces<bool>()
			.WithQueryStringParameter("Path", required: true);

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

	private static void RotateLogs(string path)
	{
		const int MaxLogFiles = 5;
		string? directory = Path.GetDirectoryName(path);
		if (directory is null)
		{
			return;
		}

		FileInfo[] logFiles = new DirectoryInfo(directory)
			.GetFiles("AssetRipper_*.log")
			.OrderBy(f => f.Name)
			.ToArray();

		for (int i = 0; i <= logFiles.Length - MaxLogFiles; i++)
		{
			try
			{
				logFiles[i].Delete();
			}
			catch (IOException)
			{
				// Could not delete log file, ignore
			}
		}
	}
}
