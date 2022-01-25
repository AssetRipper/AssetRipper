using AssetRipper.Core.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AssetRipper.Core.Updating
{
	public static class UpdateManager
	{
		private static HttpClient client;
		const string url = "https://api.github.com/repos/ds5678/AssetRipper/releases";

		static UpdateManager()
		{
			client = new HttpClient();
			ProductInfoHeaderValue product = new(BuildInfo.Name, BuildInfo.Version);
			ProductInfoHeaderValue comment = new($"(+{BuildInfo.WebsiteURL})");
			client.DefaultRequestHeaders.UserAgent.Add(product);
			client.DefaultRequestHeaders.UserAgent.Add(comment);
		}

		/// <summary>
		/// Checks Github for the latest release
		/// </summary>
		/// <param name="localVersion">The local version of this application</param>
		/// <param name="githubVersion">The latest release on Github</param>
		/// <returns>True if the github version could be acquired. False otherwise.</returns>
		public static bool CheckForUpdates(out Version localVersion, out Version githubVersion)
		{
			localVersion = Version.Parse(BuildInfo.Version);
			try
			{
				var task = client.GetFromJsonAsync<List<GithubRelease>>(url);
				task.Wait();
				List<GithubRelease> releases = task.Result;

				if (releases == null)
				{
					githubVersion = null;
					return false;
				}

				githubVersion = Version.Parse(releases[0].TagName);
				return true;
			}
			catch
			{
				githubVersion = null;
				return false;
			}
		}

		public static void LogUpdateCheck()
		{
			if (CheckForUpdates(out Version localVersion, out Version githubVersion))
			{
				if (githubVersion > localVersion)
					Logger.Info(LogCategory.System, $"A updated version is available ({githubVersion}) at {BuildInfo.LatestReleaseURL}");
			}
			else
			{
				Logger.Info(LogCategory.System, "Could not get release information from Github");
			}
		}
	}
}
