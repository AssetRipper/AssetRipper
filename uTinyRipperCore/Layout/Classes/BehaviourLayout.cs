using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout
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

		public static void GenerateTypeTree(TypeTreeContext context)
		{
			BehaviourLayout layout = context.Layout.Behaviour;
			ComponentLayout.GenerateTypeTree(context);
			context.AddByte(layout.EnabledName);
			if (layout.IsAlignEnabled)
			{
				context.Align();
			}
		}

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasEnabledInvariant => true;
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
