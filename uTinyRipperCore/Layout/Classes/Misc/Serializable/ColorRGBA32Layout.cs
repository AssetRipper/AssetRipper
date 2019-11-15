using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class ColorRGBA32Layout
	{
		public ColorRGBA32Layout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			ColorRGBA32Layout layout = context.Layout.Serialized.ColorRGBA32;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			context.AddUInt32(layout.RgbaName);
			context.EndChildren();
		}

		/// <summary>
		/// NOTE: min version is 2
		/// </summary>
		public int Version => 2;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasRGBA => true;

		public string Name => TypeTreeUtils.ColorName;
		public string RgbaName => "rgba";
	}
}
