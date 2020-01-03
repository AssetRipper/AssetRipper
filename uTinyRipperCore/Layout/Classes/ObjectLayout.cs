using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class ObjectLayout
	{
		public ObjectLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2) && !info.Flags.IsRelease())
			{
				HasHideFlag = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context)
		{
			ObjectLayout layout = context.Layout.Object;
			if (layout.HasHideFlag)
			{
				context.AddUInt32(layout.ObjectHideFlagsName);
			}
		}

		/// <summary>
		/// 2.0.0 and Not Release
		/// </summary>
		public bool HasHideFlag { get; }

		public string Name => nameof(Object);
		public string ObjectHideFlagsName => "m_ObjectHideFlags";
		public string InstanceIDName => "m_InstanceID";
		public string LocalIdentfierInFileName => "m_LocalIdentfierInFile";
	}
}
