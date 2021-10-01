using AssetRipper.Core.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Layout.Classes.Misc
{
	public sealed class KeyframeTplLayout
	{
		public KeyframeTplLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2018))
			{
				// unknown conversion
				Version = 3;
			}
			else if (TangentModeExtensions.TangentMode5Relevant(info.Version))
			{
				// TangentMode enum has been changed
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			if (info.Version.IsGreaterEqual(2, 1) && !info.Flags.IsRelease())
			{
				HasTangentMode = true;
			}
			if (info.Version.IsGreaterEqual(2018))
			{
				HasWeightedMode = true;
				HasInWeight = true;
				HasOutWeight = true;
			}
		}

		public int Version { get; }

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasTime => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasValue => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasInSlope => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasOutSlope => true;
		/// <summary>
		/// 2.1.0 and greater and Not Release
		/// </summary>
		public bool HasTangentMode { get; }
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public bool HasWeightedMode { get; }
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public bool HasInWeight { get; }
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public bool HasOutWeight { get; }

		public string TimeName => "time";
		public string ValueName => "value";
		public string InSlopeName => "inSlope";
		public string OutSlopeName => "outSlope";
		public string TangentModeName => "tangentMode";
		public string WeightedModeName => "weightedMode";
		public string InWeightName => "inWeight";
		public string OutWeightName => "outWeight";
	}
}
