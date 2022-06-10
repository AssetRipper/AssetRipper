using AssetRipper.Core.Classes.CapsuleCollider2D;
using AssetRipper.SourceGenerated.Classes.ClassID_70;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class CapsuleCollider2DExtensions
	{
		public static CapsuleDirection2D GetDirection(this ICapsuleCollider2D collider)
		{
			return (CapsuleDirection2D)collider.Direction_C70;
		}
	}
}
