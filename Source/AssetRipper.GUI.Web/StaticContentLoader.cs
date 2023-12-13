using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace AssetRipper.GUI.Web;

public static partial class StaticContentLoader
{
	private const string Prefix = "AssetRipper.GUI.Web.StaticContent.";
	private static readonly ConcurrentDictionary<string, byte[]> Cache = new();

	public static async ValueTask<byte[]> Load(string path)
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

	[GeneratedRegex(@"[/\\]")]
	private static partial Regex DirectorySeparator();
}
