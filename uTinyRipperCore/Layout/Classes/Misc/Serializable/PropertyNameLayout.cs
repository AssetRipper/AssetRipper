using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class PropertyNameLayout
	{
		public PropertyNameLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddString(name);
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasID => true;
	}
}
