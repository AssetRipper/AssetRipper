using System.Collections.Concurrent;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetRipper.GUI.Web;

public static partial class StaticContentLoader
{
	private const string Prefix = "AssetRipper.GUI.Web.StaticContent.";
	private static ConcurrentDictionary<string, byte[]> Cache { get; } = new();

	public static void Add(string path, byte[] data)
	{
		Cache.TryAdd(path, data);
	}

	public static void Add(string path, string data)
	{
		Add(path, Encoding.UTF8.GetBytes(data));
	}

	public static bool Contains(string path)
	{
		return Cache.ContainsKey(path);
	}

	public static async ValueTask<byte[]> LoadEmbedded(string path)
	{
		if (Cache.TryGetValue(path, out byte[]? result))
		{
			return result;
		}
		else
		{
			return await LoadInternal(path);
		}
	}

	public static async ValueTask<byte[]> LoadRemote(string path, string source, string? integrity = null)
	{
		if (Cache.TryGetValue(path, out byte[]? result))
		{
			return result;
		}
		else
		{
			using HttpClient client = HttpClientBuilder.CreateHttpClient();

			byte[] data;
			HttpResponseMessage response = await client.GetAsync(source);
			if (response.IsSuccessStatusCode)
			{
				data = await response.Content.ReadAsByteArrayAsync();
			}
			else
			{
				Cache.TryAdd(path, []);
				return [];
			}

			if (ValidateIntegrity(data, integrity))
			{
				Cache.TryAdd(path, data);
				return data;
			}
			else
			{
				Cache.TryAdd(path, []);
				return [];
			}
		}
	}

	private static async Task<byte[]> LoadInternal(string path)
	{
		using Stream stream = typeof(StaticContentLoader).Assembly.GetManifestResourceStream(ToResourceName(path))
			?? throw new NullReferenceException($"Could not load static file: {path}");

		using MemoryStream memoryStream = new((int)stream.Length);

		await stream.CopyToAsync(memoryStream);

		byte[] result = memoryStream.ToArray();

		Cache.TryAdd(path, result);

		return result;
	}

	private static string ToResourceName(string path)
	{
		ArgumentException.ThrowIfNullOrEmpty(path);
		string realPath = path[0] is '/' or '\\' ? path[1..] : path;
		return Prefix + DirectorySeparator().Replace(realPath, ".");
	}

	private static bool ValidateIntegrity(byte[] data, string? integrity)
	{
		if (string.IsNullOrEmpty(integrity))
		{
			return true;
		}

		const string Sha384Prefix = "sha384-";
		if (integrity.StartsWith(Sha384Prefix, StringComparison.Ordinal))
		{
			byte[] hash = SHA384.HashData(data);
			byte[] integrityHash = Convert.FromBase64String(integrity[Sha384Prefix.Length..]);
			return hash.SequenceEqual(integrityHash);
		}
		else
		{
			return false;
		}
	}

	[GeneratedRegex(@"[/\\]")]
	private static partial Regex DirectorySeparator();
}
