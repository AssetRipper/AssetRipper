using AssetRipper.SourceGenerated.Subclasses.SpriteBone;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SpriteBoneExtensions
{
	public static void CopyValues(this ISpriteBone destination, ISpriteBone source)
	{
		if (destination.Has_Color() && source.Has_Color())
		{
			destination.Color.CopyValues(source.Color);
		}
		if (destination.Has_Guid() && source.Has_Guid())
		{
			destination.Guid = source.Guid;
		}
		destination.Length = source.Length;
		destination.Name = source.Name;
		destination.ParentId = source.ParentId;
		destination.Position.CopyValues(source.Position);
		destination.Rotation.CopyValues(source.Rotation);
	}
}
