using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.SourceGenerated.Subclasses.DetailPrototype;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class DetailPrototypeExtensions
	{
		public static DetailRenderMode GetRenderMode(this IDetailPrototype info)
		{
			return (DetailRenderMode)info.RenderMode;
		}
	}
}
