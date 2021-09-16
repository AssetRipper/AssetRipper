using AssetRipper.Core.Converters.Game;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Layout.Classes.Misc.Serializable
{
	public sealed class ColorRGBA32Layout
	{
		public ColorRGBA32Layout(LayoutInfo info) { }

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			ColorRGBA32Layout layout = context.Layout.Serialized.ColorRGBA32;
			context.AddNode(layout.Name, name, ColorRGBA32.ToSerializedVersion());
			context.BeginChildren();
			context.AddUInt32(ColorRGBA32.RgbaName);
			context.EndChildren();
		}

		public string Name => TypeTreeUtils.ColorName;
	}
}
