using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class Vector2fLayout
	{
		public Vector2fLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			Vector2fLayout layout = context.Layout.Serialized.Vector2f;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddSingle(layout.XName);
			context.AddSingle(layout.YName);
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

		public string Name => TypeTreeUtils.Vector2Name;
		public string XName => "x";
		public string YName => "y";
	}
}
