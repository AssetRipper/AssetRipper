using AssetRipper.Core.Classes.CompositeCollider2D;
using AssetRipper.SourceGenerated.Classes.ClassID_66;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class CompositeCollider2DExtensions
	{
		public static GeometryType GetGeometryType(this ICompositeCollider2D collider)
		{
			return (GeometryType)collider.GeometryType_C66;
		}

		public static GenerationType GetGenerationType(this ICompositeCollider2D collider)
		{
			return (GenerationType)collider.GenerationType_C66;
		}
	}
}
