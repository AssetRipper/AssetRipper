using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class NamedObjectLayout
	{
		public NamedObjectLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context)
		{
			EditorExtensionLayout.GenerateTypeTree(context);
			NamedObjectLayout layout = context.Layout.NamedObject;
			context.AddString(layout.NameName);
		}

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasName => true;

		public string Name => nameof(NamedObject);
		public string NameName => "m_Name";
	}
}
