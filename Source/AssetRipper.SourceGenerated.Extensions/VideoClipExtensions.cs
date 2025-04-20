using AssetRipper.SourceGenerated.Classes.ClassID_329;

namespace AssetRipper.SourceGenerated.Extensions;

public static class VideoClipExtensions
{
	public static bool CheckIntegrity(this IVideoClip clip)
	{
		return clip.ExternalResources.CheckIntegrity(clip.Collection);
	}

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
		return clip.TryGetExtensionFromPath(out string? extension) ? extension : "bytes";
	}

	public static bool TryGetExtensionFromPath(this IVideoClip clip, [NotNullWhen(true)] out string? extension)
	{
		extension = Path.GetExtension(clip.OriginalPath_R);
		if (string.IsNullOrEmpty(extension))
		{
			extension = null;
			return false;
		}
		extension = extension[1..];
		return true;
	}

	public static string? TryGetExtensionFromPath(this IVideoClip clip)
	{
		return clip.TryGetExtensionFromPath(out string? extension) ? extension : null;
	}
}
