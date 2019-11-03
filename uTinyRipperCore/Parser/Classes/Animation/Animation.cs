using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Classes.Animations;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class Animation : Behaviour
	{
		public Animation(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(3, 4))
			{
				return 3;
			}
			if (version.IsGreaterEqual(1, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 1.5
		/// </summary>
		public static bool HasAnimationsPaired(Version version) => version.IsLess(1, 5);
		/// <summary>
		/// 2.6.0 to 3.4.0 exclusive
		/// </summary>
		public static bool HasAnimateOnlyIfVisible(Version version) => version.IsGreaterEqual(2, 6) && version.IsLess(3, 4);
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasCullingType(Version version) => version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 3.4.0 to
		/// </summary>
		public static bool HasUserAABB(Version version) => version.IsGreaterEqual(3, 4) && version.IsLess(4, 3);

		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(3, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			DefaultAnimation.Read(reader);
			if (HasAnimationsPaired(reader.Version))
			{
				AnimationsPaired = reader.ReadTupleStringTArray<PPtr<AnimationClip>>();
			}
			else
			{
				Animations = reader.ReadAssetArray<PPtr<AnimationClip>>();
			}
			WrapMode = (WrapMode)reader.ReadInt32();
			PlayAutomatically = reader.ReadBoolean();
			AnimatePhysics = reader.ReadBoolean();
			if(HasAnimateOnlyIfVisible(reader.Version))
			{
				AnimateOnlyIfVisible = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasCullingType(reader.Version))
			{
				CullingType = (AnimationCullingType)reader.ReadInt32();
			}

			if (HasUserAABB(reader.Version))
			{
				UserAABB.Read(reader);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			yield return context.FetchDependency(DefaultAnimation, AnimationName);

			if (HasAnimationsPaired(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(AnimationsPaired.Select(t => t.Item2), AnimationsName))
				{
					yield return asset;
				}
			}
			else
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(Animations, AnimationsName))
				{
					yield return asset;
				}
			}
		}
		
		public bool IsContainsAnimationClip(AnimationClip clip)
		{
			foreach (PPtr<AnimationClip> clipPtr in Animations)
			{
				if (clipPtr.IsAsset(File, clip))
				{
					return true;
				}
			}
			return false;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AnimationName, DefaultAnimation.ExportYAML(container));
			node.Add(AnimationsName, Animations.ExportYAML(container));
			node.Add(WrapModeName, (int)WrapMode);
			node.Add(PlayAutomaticallyName, PlayAutomatically);
			node.Add(AnimatePhysicsName, AnimatePhysics);
			node.Add(CullingTypeName, (int)CullingType);
			return node;
		}
		
		public Tuple<string, PPtr<AnimationClip>>[] AnimationsPaired { get; set; }
		public PPtr<AnimationClip>[] Animations { get; set; }
		public WrapMode WrapMode { get; set; }
		public bool PlayAutomatically { get; set; }
		/// <summary>
		/// PlayFixedFrameRate previously
		/// </summary>
		public bool AnimatePhysics { get; set; }
		public bool AnimateOnlyIfVisible { get; set; }
		public AnimationCullingType CullingType { get; set; }

		public const string AnimationName = "m_Animation";
		public const string AnimationsName = "m_Animations";
		public const string WrapModeName = "m_WrapMode";
		public const string PlayAutomaticallyName = "m_PlayAutomatically";
		public const string AnimatePhysicsName = "m_AnimatePhysics";
		public const string CullingTypeName = "m_CullingType";

		public PPtr<AnimationClip> DefaultAnimation;
		public AABB UserAABB;
	}
}
