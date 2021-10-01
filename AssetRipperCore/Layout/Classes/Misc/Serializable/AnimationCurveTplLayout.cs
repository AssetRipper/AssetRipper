
namespace AssetRipper.Core.Layout.Classes.Misc.Serializable
{
	public sealed class AnimationCurveTplLayout
	{
		public AnimationCurveTplLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2, 1))
			{
				// unknown conversion
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			if (info.Version.IsGreaterEqual(5, 3))
			{
				HasRotationOrder = true;
			}
		}

		public int Version { get; }

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCurve => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPreInfinity => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPostInfinity => true;
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public bool HasRotationOrder { get; }

		public string CurveName => "m_Curve";
		public string PreInfinityName => "m_PreInfinity";
		public string PostInfinityName => "m_PostInfinity";
		public string RotationOrderName => "m_RotationOrder";
	}
}
