using AssetRipper.SourceGenerated.Classes.ClassID_90;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AvatarExtensions
	{
		public static Utf8String? FindBonePath(this IAvatar avatar, uint hash)
		{
			avatar.TOS_C90.TryGetValue(hash, out Utf8String? result);
			return result;
		}
	}
}
