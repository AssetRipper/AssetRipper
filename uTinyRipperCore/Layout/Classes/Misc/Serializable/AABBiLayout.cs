using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class AABBiLayout
	{
		public AABBiLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			AABBiLayout layout = context.Layout.Serialized.AABBi;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			Vector3fLayout.GenerateTypeTree(context, layout.CenterName);
			Vector3fLayout.GenerateTypeTree(context, layout.ExtentName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCenter => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasExtent => true;

		public string Name => TypeTreeUtils.BoundsIntName;
		public string CenterName => "m_Center";
		public string ExtentName => "m_Extent";
	}
}
