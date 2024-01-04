using AssetRipper.GUI.Web.Pages;
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
using System.Diagnostics;

namespace AssetRipper.GUI.Web;

public static class WebApplicationLauncher
{
	public static void Launch()
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

		builder.WebHost.UseUrls("http://127.0.0.1:0");

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
		app.Lifetime.ApplicationStarted.Register(() =>
		{
			string? address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
			if (address is not null)
			{
				OpenUrl(address);
			}
		});

		//Static files
		app.MapGet("/favicon.ico", async () =>
		{
			byte[] data = await StaticContentLoader.Load("favicon.ico");
			return Results.Bytes(data, "image/x-icon");
		});
		app.MapGet("/css/site.css", async () =>
		{
			byte[] data = await StaticContentLoader.Load("css/site.css");
			return Results.Bytes(data, "text/css");
		});
		app.MapGet("/js/site.js", async () =>
		{
			byte[] data = await StaticContentLoader.Load("js/site.js");
			return Results.Bytes(data, "text/javascript");
		});

		//Normal Pages
		app.MapGet("/", (context) =>
		{
			context.Response.DisableCaching();
			return IndexPage.Instance.WriteToResponse(context.Response);
		});
		app.MapGet("/Commands", () => CommandsPage.Instance.ToResult());
		app.MapGet("/Privacy", () => PrivacyPage.Instance.ToResult());
		app.MapGet("/Licenses", () => LicensesPage.Instance.ToResult());
		app.MapGet("/Settings/Edit", (context) =>
		{
			context.Response.DisableCaching();
			return SettingsPage.Instance.WriteToResponse(context.Response);
		});
		app.MapPost("/Settings/Update", SettingsPage.HandlePostRequest);
		app.MapPost("/Assets/View", Pages.Assets.ViewPage.HandlePostRequest);
		app.MapPost("/Bundles/View", Pages.Bundles.ViewPage.HandlePostRequest);
		app.MapPost("/Collections/View", Pages.Collections.ViewPage.HandlePostRequest);
		app.MapPost("/Resources/View", Pages.Resources.ViewPage.HandlePostRequest);
		app.MapPost("/Scenes/View", Pages.Scenes.ViewPage.HandlePostRequest);

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
}
