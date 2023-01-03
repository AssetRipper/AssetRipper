using AssetRipper.SourceGenerated.Subclasses.SpriteBone;

namespace AssetRipper.Core.SourceGenExtensions
{
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
				destination.Guid.CopyValues(source.Guid);
			}
			destination.Length = source.Length;
			destination.Name_R.CopyValues(source.Name_R);
			destination.ParentId = source.ParentId;
			destination.Position.CopyValues(source.Position);
			destination.Rotation.CopyValues(source.Rotation);
		}
	}
}
