using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class Vector3fLayout
	{
		public Vector3fLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			Vector3fLayout layout = context.Layout.Serialized.Vector3f;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddSingle(layout.XName);
			context.AddSingle(layout.YName);
			context.AddSingle(layout.ZName);
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

		public string Name => TypeTreeUtils.Vector3Name;
		public string XName => "x";
		public string YName => "y";
		public string ZName => "z";
	}
}
