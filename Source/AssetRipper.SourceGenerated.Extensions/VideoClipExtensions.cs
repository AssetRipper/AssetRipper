using AssetRipper.SourceGenerated.Classes.ClassID_329;

namespace AssetRipper.SourceGenerated.Extensions;

public static class VideoClipExtensions
{
	public static bool TryGetContent(this IVideoClip clip, [NotNullWhen(true)] out byte[]? data)
	{
		return clip.ExternalResources.TryGetContent(clip.Collection, out data);
	}

	public static byte[] GetContent(this IVideoClip clip)
	{
		return clip.ExternalResources.GetContent(clip.Collection) ?? [];
	}

	public static string GetExtensionFromPath(this IVideoClip clip)
	{
		string extension = Path.GetExtension(clip.OriginalPath_R);
		return string.IsNullOrEmpty(extension) ? "bytes" : extension[1..];
	}
}
