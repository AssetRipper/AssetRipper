using AssetRipper.Import.Logging;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.Project;

/// <summary>
/// Result of querying the Unity Registry for a package.
/// </summary>
public sealed record PackageVersionInfo(string Version, string? TarballUrl);

/// <summary>
/// Client for the Unity Package Registry (packages.unity.com).
/// Used to resolve correct package versions for a given Unity version.
/// </summary>
public sealed class UnityRegistryClient : IDisposable
{
	private const string RegistryUrl = "https://packages.unity.com";

	private readonly HttpClient _httpClient;

	public UnityRegistryClient()
	{
		_httpClient = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(30),
		};
	}

	/// <summary>
	/// Query the registry for a package and find the best compatible version for the given Unity version.
	/// </summary>
	/// <returns>The best compatible version string, or null if the package was not found or no compatible version exists.</returns>
	public string? GetCompatibleVersion(string packageName, UnityVersion projectUnityVersion)
	{
		return GetCompatibleVersionInfo(packageName, projectUnityVersion)?.Version;
	}

	/// <summary>
	/// Query the registry for a package and find the best compatible version, including tarball URL.
	/// </summary>
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
	/// Get the tarball URL for any version of a package (ignoring compatibility).
	/// Used to extract .asmdef GUIDs for embedded/fallback packages where no compatible version exists.
	/// GUIDs are stable across all versions of a package.
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

			// Just grab the first version's tarball URL
			foreach (JsonProperty versionEntry in versionsElement.EnumerateObject())
			{
				JsonElement versionData = versionEntry.Value;
				if (versionData.TryGetProperty("dist", out JsonElement distElement)
					&& distElement.TryGetProperty("tarball", out JsonElement tarballElement))
				{
					return tarballElement.GetString();
				}
			}

			return null;
		}
		catch (Exception ex)
		{
			Logger.Warning(LogCategory.Export, $"Unity Registry: Failed to get tarball URL for '{packageName}': {ex.Message}");
			return null;
		}
	}

	private JsonDocument? FetchPackageDocument(string packageName)
	{
		string url = $"{RegistryUrl}/{packageName}";
		using HttpResponseMessage response = _httpClient.GetAsync(url).GetAwaiter().GetResult();

		if (!response.IsSuccessStatusCode)
		{
			return null;
		}

		using Stream stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
		return JsonDocument.Parse(stream);
	}

	/// <summary>
	/// Download a package tarball and extract per-script GUIDs from .cs.meta files.
	/// Unity packages are source-compiled, so script references use the .cs.meta GUID
	/// (with fileID 11500000), not the .asmdef GUID.
	/// </summary>
	/// <returns>Dictionary mapping class names to their .cs.meta GUIDs.</returns>
	public Dictionary<string, UnityGuid> ExtractScriptGuids(string tarballUrl)
	{
		Dictionary<string, UnityGuid> guids = new(StringComparer.Ordinal);

		try
		{
			using HttpResponseMessage response = _httpClient.GetAsync(tarballUrl).GetAwaiter().GetResult();
			if (!response.IsSuccessStatusCode)
			{
				Logger.Warning(LogCategory.Export, $"Unity Registry: Failed to download tarball: {response.StatusCode}");
				return guids;
			}

			using Stream httpStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
			using GZipStream gzipStream = new(httpStream, CompressionMode.Decompress);
			using TarReader tarReader = new(gzipStream);

			while (tarReader.GetNextEntry() is TarEntry entry)
			{
				if (entry.DataStream is null)
				{
					continue;
				}

				string entryName = entry.Name;

				// Match .cs.meta files (but not .asmdef.meta, .shader.meta, etc.)
				if (entryName.EndsWith(".cs.meta", StringComparison.OrdinalIgnoreCase))
				{
					string metaContent = ReadStreamAsString(entry.DataStream);
					UnityGuid? guid = ParseGuidFromMeta(metaContent);
					if (guid is not null)
					{
						// Extract class name from file path: "package/Scripts/Runtime/TextMeshProUGUI.cs.meta" → "TextMeshProUGUI"
						string fileName = Path.GetFileName(entryName); // "TextMeshProUGUI.cs.meta"
						string className = fileName[..^".cs.meta".Length]; // "TextMeshProUGUI"

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

	private static UnityGuid? ParseGuidFromMeta(string metaContent)
	{
		// Meta files are YAML-like, look for "guid: <hex>"
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

	private static string? ParseAssemblyNameFromAsmdef(string asmdefContent)
	{
		try
		{
			using JsonDocument doc = JsonDocument.Parse(asmdefContent);
			if (doc.RootElement.TryGetProperty("name", out JsonElement nameElement))
			{
				return nameElement.GetString();
			}
		}
		catch
		{
			// Invalid JSON
		}
		return null;
	}

	/// <summary>
	/// Find the latest non-preview version that is compatible with the project's Unity version.
	/// Falls back to the latest version if no stable match is found.
	/// </summary>
	private static PackageVersionInfo? FindBestVersion(JsonElement root, UnityVersion projectUnityVersion, string packageName)
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
			string versionString = versionEntry.Name;
			JsonElement versionData = versionEntry.Value;

			// Parse the minimum Unity version required by this package version
			UnityVersion minUnityVersion = default;
			if (versionData.TryGetProperty("unity", out JsonElement unityElement))
			{
				string? unityStr = unityElement.GetString();
				if (!string.IsNullOrEmpty(unityStr))
				{
					try
					{
						minUnityVersion = UnityVersion.Parse(unityStr);
					}
					catch
					{
						// Can't parse, treat as compatible with any version
					}
				}
			}

			// Check if this version is compatible with the project's Unity version
			if (minUnityVersion != default && projectUnityVersion < minUnityVersion)
			{
				continue; // This package version requires a newer Unity
			}

			// Extract tarball URL
			string? tarballUrl = null;
			if (versionData.TryGetProperty("dist", out JsonElement distElement)
				&& distElement.TryGetProperty("tarball", out JsonElement tarballElement))
			{
				tarballUrl = tarballElement.GetString();
			}

			bool isPreRelease = versionString.Contains('-');

			if (!isPreRelease)
			{
				if (bestStableVersion is null || CompareVersionStrings(versionString, bestStableVersion) > 0)
				{
					bestStableVersion = versionString;
					bestStableTarball = tarballUrl;
				}
			}

			// Track best of any version (including pre-release) as fallback
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

	/// <summary>
	/// Compare two semver-like version strings. Returns positive if a > b.
	/// </summary>
	private static int CompareVersionStrings(string a, string b)
	{
		// Split off pre-release suffix
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

		// If base versions are equal, non-prerelease > prerelease
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
		_httpClient.Dispose();
	}
}
