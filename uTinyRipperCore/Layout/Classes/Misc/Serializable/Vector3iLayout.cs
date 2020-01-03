using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class Vector3iLayout
	{
		public Vector3iLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			Vector3iLayout layout = context.Layout.Serialized.Vector3i;
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

		public string Name => TypeTreeUtils.Vector3IntName;
		public string XName => "x";
		public string YName => "y";
		public string ZName => "z";
	}
}
