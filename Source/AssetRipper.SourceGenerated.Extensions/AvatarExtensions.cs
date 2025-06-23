using AssetRipper.SourceGenerated.Classes.ClassID_90;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AvatarExtensions
{
	public static Utf8String? FindBonePath(this IAvatar avatar, uint hash)
	{
		avatar.TOS.TryGetValue(hash, out Utf8String? result);
		return result;
	}
}
