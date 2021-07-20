using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.GameObject;

namespace AssetRipper.Converters.Classes.GameObject
{
	public static class ComponentPairConverter
	{
		public static ComponentPair Convert(IExportContainer container, ComponentPair origin)
		{
			return origin;
		}
	}
}
