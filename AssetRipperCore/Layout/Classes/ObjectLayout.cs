using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Layout.Classes
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
