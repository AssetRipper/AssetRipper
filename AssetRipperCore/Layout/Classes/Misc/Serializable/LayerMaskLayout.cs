
namespace AssetRipper.Core.Layout.Classes.Misc.Serializable
{
	public sealed class LayerMaskLayout
	{
		public LayerMaskLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2))
			{
				// Bits size has been changed to 32
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			Is32Bits = info.Version.IsGreaterEqual(2);
		}

		public int Version { get; }

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool Is32Bits { get; }

		public string BitsName => "m_Bits";
	}
}
