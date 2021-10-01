using AssetRipper.Core.Classes.Animation;
using AssetRipper.Core.Layout.Classes.Misc.Serializable;

namespace AssetRipper.Core.Layout.Classes
{
	public sealed class AnimationLayout
	{
		public AnimationLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(3, 4))
			{
				// AnimateOnlyIfVisible has been replaced by CullingType
				Version = 3;
			}
			else if (info.Version.IsGreaterEqual(1, 5))
			{
				// PlayFixedFrameRate has been renamed to AnimatePhysics
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			// Fields
			if (info.Version.IsGreaterEqual(1, 5))
			{
				HasAnimations = true;
			}
			else
			{
				HasAnimationsPaired = true;
			}
			if (info.Version.IsGreaterEqual(1, 5))
			{
				HasAnimatePhysics = true;
			}
			else
			{
				HasPlayFixedFrameRate = true;
			}
			if (info.Version.IsGreaterEqual(2, 6))
			{
				HasCullingTypeInvariant = true;
				if (info.Version.IsGreaterEqual(3, 4))
				{
					HasCullingType = true;
				}
				else
				{
					HasAnimateOnlyIfVisible = true;
				}
			}
			if (info.Version.IsGreaterEqual(3, 4) && info.Version.IsLess(4, 3))
			{
				HasUserAABB = true;
			}

			// Flags
			if (info.Version.IsGreaterEqual(3, 2))
			{
				IsAlign = true;
			}

			// Names
			if (HasAnimatePhysics)
			{
				AnimatePhysicsInvariantName = AnimatePhysicsName;
			}
			else
			{
				AnimatePhysicsInvariantName = PlayFixedFrameRateName;
			}
		}

		public int Version { get; }

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasDefaultAnimation => true;
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public bool HasAnimations { get; }
		/// <summary>
		/// Less than 1.5.0
		/// </summary>
		public bool HasAnimationsPaired { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasWrapMode => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPlayAutomatically => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAnimatePhysicsInvariant => true;
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public bool HasAnimatePhysics { get; }
		/// <summary>
		/// Less than 1.5.0
		/// </summary>
		public bool HasPlayFixedFrameRate { get; }
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public bool HasCullingTypeInvariant { get; }
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public bool HasCullingType { get; }
		/// <summary>
		/// 2.6.0 to 3.4.0 exclusive
		/// </summary>
		public bool HasAnimateOnlyIfVisible { get; }
		/// <summary>
		/// 3.4.0 to 4.3.0 exclusive
		/// </summary>
		public bool HasUserAABB { get; }

		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public bool IsAlign { get; }

		public string Name => nameof(Animation);
		public string AnimationName => "m_Animation";
		public string AnimationsName => "m_Animations";
		public string WrapModeName => "m_WrapMode";
		public string PlayAutomaticallyName => "m_PlayAutomatically";
		public string AnimatePhysicsInvariantName { get; }
		public string AnimatePhysicsName => "m_AnimatePhysics";
		public string PlayFixedFrameRateName => "m_PlayFixedFrameRate";
		public string CullingTypeName => "m_CullingType";
		public string AnimateOnlyIfVisibleName => "m_AnimateOnlyIfVisible";
		public string UserAABBName => "m_UserAABB";
	}
}
