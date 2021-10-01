using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Layout.Classes.Misc.Serializable.GUIStyle
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

		public string BackgroundName => "m_Background";
		public string ScaledBackgroundsName => "m_ScaledBackgrounds";
		public string TextColorName => "m_TextColor";
	}
}
