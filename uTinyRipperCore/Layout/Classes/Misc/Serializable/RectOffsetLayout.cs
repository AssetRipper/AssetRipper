using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class RectOffsetLayout
	{
		public RectOffsetLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			RectOffsetLayout layout = context.Layout.Serialized.RectOffset;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddInt32(layout.LeftName);
			context.AddInt32(layout.RightName);
			context.AddInt32(layout.TopName);
			context.AddInt32(layout.BottomName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool Left => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool Right => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool Top => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool Bottom => true;

		public string Name => TypeTreeUtils.RectOffsetName;
		public string LeftName => "m_Left";
		public string RightName => "m_Right";
		public string TopName => "m_Top";
		public string BottomName => "m_Bottom";
	}
}
