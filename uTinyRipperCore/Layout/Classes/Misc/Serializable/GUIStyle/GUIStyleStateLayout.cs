using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class GUIStyleStateLayout
	{
		public GUIStyleStateLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(5, 4) && !info.Flags.IsRelease())
			{
				HasScaledBackgrounds = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			GUIStyleStateLayout layout = context.Layout.Serialized.GUIStyle.GUIStyleState;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddPPtr(context.Layout.Texture2D.Name, layout.BackgroundName);
			if (layout.HasScaledBackgrounds)
			{
				context.AddArray(layout.ScaledBackgroundsName, (c, n) => c.AddPPtr(c.Layout.Texture2D.Name, n));
			}
			ColorRGBAfLayout.GenerateTypeTree(context, layout.TextColorName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasBackground => true;
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public bool HasScaledBackgrounds { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasTextColor => true;

		public string Name => TypeTreeUtils.GUIStyleStateName;
		public string BackgroundName => "m_Background";
		public string ScaledBackgroundsName => "m_ScaledBackgrounds";
		public string TextColorName => "m_TextColor";
	}
}
