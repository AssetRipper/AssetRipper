using AssetRipper.Import.Logging;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.Project;

internal sealed record PackageVersionInfo(string Version, string? TarballUrl);

internal sealed class UnityRegistryClient : IDisposable
{
	private const string RegistryUrl = "https://packages.unity.com";

	private readonly HttpClient httpClient;

	public UnityRegistryClient()
	{
		httpClient = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(30),
		};
	}

	public PackageVersionInfo? GetCompatibleVersionInfo(string packageName, UnityVersion projectUnityVersion)
	{
		try
		{
			using JsonDocument? doc = FetchPackageDocument(packageName);
			if (doc is null)
			{
				return null;
			}

			return FindBestVersion(doc.RootElement, projectUnityVersion, packageName);
		}
		catch (HttpRequestException ex)
		{
			Logger.Warning(LogCategory.Export, $"Unity Registry: Failed to query '{packageName}': {ex.Message}");
			return null;
		}
		catch (TaskCanceledException)
		{
			Logger.Warning(LogCategory.Export, $"Unity Registry: Timeout querying '{packageName}'.");
			return null;
		}
		catch (JsonException ex)
		{
			Logger.Warning(LogCategory.Export, $"Unity Registry: Failed to parse response for '{packageName}': {ex.Message}");
			return null;
		}
	}

	/// <summary>
	/// Get a tarball URL for any version of the package, ignoring Unity-version compatibility.
	/// Used for embedded packages where no compatible version exists on the registry but we still
	/// want script GUIDs — GUIDs are stable across all package versions.
	/// </summary>
	public string? GetAnyTarballUrl(string packageName)
	{
		try
		{
			using JsonDocument? doc = FetchPackageDocument(packageName);
			if (doc is null)
			{
				return null;
			}

			if (!doc.RootElement.TryGetProperty("versions", out JsonElement versionsElement))
			{
				return null;
			}

			return versionsElement.EnumerateObject()
				.Select(versionEntry => TryGetTarballUrl(versionEntry.Value))
				.FirstOrDefault(url => url is not null);
		}
		catch (Exception ex)
		{
			Logger.Warning(LogCategory.Export, $"Unity Registry: Failed to get tarball URL for '{packageName}': {ex.Message}");
			return null;
		}
	}

	private JsonDocument? FetchPackageDocument(string packageName)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"{RegistryUrl}/{packageName}");
		using HttpResponseMessage response = httpClient.Send(request);

		if (!response.IsSuccessStatusCode)
		{
			return null;
		}

		using Stream stream = response.Content.ReadAsStream();
		return JsonDocument.Parse(stream);
	}

	/// <summary>
	/// Download a package tarball and extract per-script GUIDs from .cs.meta files.
	/// </summary>
	public Dictionary<string, UnityGuid> ExtractScriptGuids(string tarballUrl)
	{
		Dictionary<string, UnityGuid> guids = new(StringComparer.Ordinal);

		try
		{
			using HttpRequestMessage request = new(HttpMethod.Get, tarballUrl);
			using HttpResponseMessage response = httpClient.Send(request);
			if (!response.IsSuccessStatusCode)
			{
				Logger.Warning(LogCategory.Export, $"Unity Registry: Failed to download tarball: {response.StatusCode}");
				return guids;
			}

			using Stream httpStream = response.Content.ReadAsStream();
			using GZipStream gzipStream = new(httpStream, CompressionMode.Decompress);
			using TarReader tarReader = new(gzipStream);

			while (tarReader.GetNextEntry() is TarEntry entry)
			{
				if (entry.DataStream is null)
				{
					continue;
				}

				string entryName = entry.Name;

				// .asmdef.meta, .shader.meta, etc. would also end with .meta — only .cs.meta carries script GUIDs.
				if (entryName.EndsWith(".cs.meta", StringComparison.OrdinalIgnoreCase))
				{
					string metaContent = ReadStreamAsString(entry.DataStream);
					UnityGuid? guid = ParseGuidFromMeta(metaContent);
					if (guid is not null)
					{
						string fileName = Path.GetFileName(entryName);
						string className = fileName[..^".cs.meta".Length];

						if (!string.IsNullOrEmpty(className))
						{
							guids[className] = guid.Value;
						}
					}
				}
			}

			Logger.Info(LogCategory.Export, $"  Extracted {guids.Count} script GUID(s) from tarball");
		}
		catch (Exception ex)
		{
			Logger.Warning(LogCategory.Export, $"Unity Registry: Failed to extract script GUIDs from tarball: {ex.Message}");
		}

		return guids;
	}

	private static string ReadStreamAsString(Stream stream)
	{
		using StreamReader reader = new(stream, leaveOpen: true);
		return reader.ReadToEnd();
	}

	internal static UnityGuid? ParseGuidFromMeta(string metaContent)
	{
		foreach (string line in metaContent.Split('\n'))
		{
			string trimmed = line.Trim();
			if (trimmed.StartsWith("guid:", StringComparison.Ordinal))
			{
				string guidStr = trimmed["guid:".Length..].Trim();
				if (guidStr.Length == 32)
				{
					try
					{
						return UnityGuid.Parse(guidStr);
					}
					catch
					{
						// Invalid GUID format
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Find the latest non-prerelease version compatible with the project's Unity version.
	/// Falls back to the latest prerelease if no stable match exists, since some packages
	/// (e.g. early-preview ones) ship without a stable release.
	/// </summary>
	internal static PackageVersionInfo? FindBestVersion(JsonElement root, UnityVersion projectUnityVersion, string packageName)
	{
		if (!root.TryGetProperty("versions", out JsonElement versionsElement))
		{
			return null;
		}

		string? bestStableVersion = null;
		string? bestStableTarball = null;
		string? bestAnyVersion = null;
		string? bestAnyTarball = null;

		foreach (JsonProperty versionEntry in versionsElement.EnumerateObject())
		{
			if (!IsCompatibleVersion(versionEntry.Value, projectUnityVersion))
			{
				continue;
			}

			string versionString = versionEntry.Name;
			string? tarballUrl = TryGetTarballUrl(versionEntry.Value);
			bool isPreRelease = versionString.Contains('-');

			if (!isPreRelease && (bestStableVersion is null || CompareVersionStrings(versionString, bestStableVersion) > 0))
			{
				bestStableVersion = versionString;
				bestStableTarball = tarballUrl;
			}

			if (bestAnyVersion is null || CompareVersionStrings(versionString, bestAnyVersion) > 0)
			{
				bestAnyVersion = versionString;
				bestAnyTarball = tarballUrl;
			}
		}

		string? version = bestStableVersion ?? bestAnyVersion;
		string? tarball = bestStableVersion is not null ? bestStableTarball : bestAnyTarball;

		return version is not null ? new PackageVersionInfo(version, tarball) : null;
	}

	private static bool IsCompatibleVersion(JsonElement versionData, UnityVersion projectUnityVersion)
	{
		if (!versionData.TryGetProperty("unity", out JsonElement unityElement))
		{
			return true;
		}

		string? unityStr = unityElement.GetString();
		if (string.IsNullOrEmpty(unityStr))
		{
			return true;
		}

		try
		{
			UnityVersion minUnityVersion = UnityVersion.Parse(unityStr);
			return projectUnityVersion >= minUnityVersion;
		}
		catch
		{
			// Unparseable, treat as compatible with any Unity version
			return true;
		}
	}

	private static string? TryGetTarballUrl(JsonElement versionData)
	{
		if (versionData.TryGetProperty("dist", out JsonElement distElement)
			&& distElement.TryGetProperty("tarball", out JsonElement tarballElement))
		{
			return tarballElement.GetString();
		}
		return null;
	}

	/// <summary>
	/// Compare two semver-like version strings. Returns positive if a &gt; b.
	/// Stable (no '-' suffix) is treated as greater than prerelease for the same base version.
	/// </summary>
	internal static int CompareVersionStrings(string a, string b)
	{
		string aBase = a.Contains('-') ? a[..a.IndexOf('-')] : a;
		string bBase = b.Contains('-') ? b[..b.IndexOf('-')] : b;

		string[] aParts = aBase.Split('.');
		string[] bParts = bBase.Split('.');

		int maxLen = Math.Max(aParts.Length, bParts.Length);
		for (int i = 0; i < maxLen; i++)
		{
			int aNum = i < aParts.Length && int.TryParse(aParts[i], out int av) ? av : 0;
			int bNum = i < bParts.Length && int.TryParse(bParts[i], out int bv) ? bv : 0;

			if (aNum != bNum)
			{
				return aNum - bNum;
			}
		}

		bool aPreRelease = a.Contains('-');
		bool bPreRelease = b.Contains('-');
		if (aPreRelease != bPreRelease)
		{
			return aPreRelease ? -1 : 1;
		}

		return string.Compare(a, b, StringComparison.Ordinal);
	}

	public void Dispose()
	{
		httpClient.Dispose();
	}
}
