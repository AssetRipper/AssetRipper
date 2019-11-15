using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class QuaternionfLayout
	{
		public QuaternionfLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			QuaternionfLayout layout = context.Layout.Serialized.Quaternionf;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddSingle(layout.XName);
			context.AddSingle(layout.XName);
			context.AddSingle(layout.ZName);
			context.AddSingle(layout.WName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool X => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool Y => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool Z => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool W => true;

		public string Name => TypeTreeUtils.QuaternionName;
		public string XName => "x";
		public string YName => "y";
		public string ZName => "z";
		public string WName => "w";
	}
}
