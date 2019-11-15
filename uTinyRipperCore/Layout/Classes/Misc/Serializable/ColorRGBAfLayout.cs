using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class ColorRGBAfLayout
	{
		public ColorRGBAfLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			ColorRGBAfLayout layout = context.Layout.Serialized.ColorRGBAf;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddSingle(layout.RName);
			context.AddSingle(layout.GName);
			context.AddSingle(layout.BName);
			context.AddSingle(layout.AName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasR => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasG => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasB => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasA => true;

		public string Name => TypeTreeUtils.ColorName;
		public string RName => "r";
		public string GName => "g";
		public string BName => "b";
		public string AName => "a";
	}
}
