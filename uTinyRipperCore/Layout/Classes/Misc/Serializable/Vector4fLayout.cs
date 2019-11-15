using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class Vector4fLayout
	{
		public Vector4fLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			Vector4fLayout layout = context.Layout.Serialized.Vector4f;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddSingle(layout.XName);
			context.AddSingle(layout.YName);
			context.AddSingle(layout.ZName);
			context.AddSingle(layout.WName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasX => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasY => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasZ => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasW => true;

		public string Name => TypeTreeUtils.Vector4Name;
		public string XName => "x";
		public string YName => "y";
		public string ZName => "z";
		public string WName => "w";
	}
}
