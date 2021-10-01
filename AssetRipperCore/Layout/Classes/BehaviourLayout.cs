using AssetRipper.Core.Classes;

namespace AssetRipper.Core.Layout.Classes
{
	public sealed class BehaviourLayout
	{
		public BehaviourLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(3))
			{
				HasEnabled = true;
			}
			else
			{
				HasEnabledBool = true;
			}

			if (info.Version.IsGreaterEqual(2, 1))
			{
				IsAlignEnabled = true;
			}
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public bool HasEnabled { get; }
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public bool HasEnabledBool { get; }

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public bool IsAlignEnabled { get; }

		public string Name => nameof(Behaviour);
		public string EnabledName => "m_Enabled";
	}
}
