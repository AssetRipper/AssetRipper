using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout
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

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			AnimationLayout layout = context.Layout.Animation;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			BehaviourLayout.GenerateTypeTree(context);
			context.AddPPtr(context.Layout.AnimationClip.Name, layout.AnimationName);
			if (layout.HasAnimations)
			{
				context.AddArray(layout.AnimationName, (c, n) => c.AddPPtr(c.Layout.AnimationClip.Name, n));
			}
			else
			{
				context.AddArray(layout.AnimationName, TupleLayout.GenerateTypeTree, StringLayout.GenerateTypeTree,
					(c, n) => c.AddPPtr(c.Layout.AnimationClip.Name, n));
			}

			context.AddInt32(layout.WrapModeName);
			context.AddBool(layout.PlayAutomaticallyName);
			context.AddBool(layout.AnimatePhysicsInvariantName);
			if (layout.HasAnimateOnlyIfVisible)
			{
				context.AddBool(layout.AnimateOnlyIfVisibleName);
			}
			if (layout.IsAlign)
			{
				context.Align();
			}

			if (layout.HasCullingType)
			{
				context.AddInt32(layout.CullingTypeName);
			}
			if (layout.HasUserAABB)
			{
				AABBLayout.GenerateTypeTree(context, layout.UserAABBName);
			}
			context.EndChildren();
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
