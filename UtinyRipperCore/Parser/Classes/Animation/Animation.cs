using System;
using System.Collections.Generic;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimationClips;
using UtinyRipper.Classes.Animations;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class Animation : Behaviour
	{
		public Animation(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 1.5
		/// </summary>
		public static bool IsReadAnimationsPaired(Version version)
		{
			return version.IsLess(1, 5);
		}
		/// <summary>
		/// 2.6.0 to 3.4.0 exclusive
		/// </summary>
		public static bool IsReadAnimateOnlyIfVisible(Version version)
		{
			return version.IsGreaterEqual(2, 6) && version.IsLess(3, 4);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadCullingType(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 3.4.0 to
		/// </summary>
		public static bool IsReadUserAABB(Version version)
		{
			return version.IsGreaterEqual(3, 4) && version.IsLess(4, 3);
		}

		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			DefaultAnimation.Read(stream);
			if (IsReadAnimationsPaired(stream.Version))
			{
				m_animationsPaired = stream.ReadTupleStringTArray<PPtr<AnimationClip>>();
			}
			else
			{
				m_animations = stream.ReadArray<PPtr<AnimationClip>>();
			}
			WrapMode = (WrapMode)stream.ReadInt32();
			PlayAutomatically = stream.ReadBoolean();
			AnimatePhysics = stream.ReadBoolean();
			if(IsReadAnimateOnlyIfVisible(stream.Version))
			{
				AnimateOnlyIfVisible = stream.ReadBoolean();
			}
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadCullingType(stream.Version))
			{
				CullingType = (AnimationCullingType)stream.ReadInt32();
			}

			if (IsReadUserAABB(stream.Version))
			{
				UserAABB.Read(stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			yield return DefaultAnimation.FetchDependency(file, isLog, ToLogString, "m_DefaultAnimation");

			if (IsReadAnimationsPaired(file.Version))
			{
				foreach(PPtr<AnimationClip> clip in AnimationsPaired.Select(t => t.Item2))
				{
					yield return clip.FetchDependency(file, isLog, ToLogString, "m_Animations");
				}
			}
			else
			{
				foreach(PPtr<AnimationClip> clip in Animations)
				{
					yield return clip.FetchDependency(file, isLog, ToLogString, "m_Animations");
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Animation", DefaultAnimation.ExportYAML(container));
			node.Add("m_Animations", Animations.ExportYAML(container));
			node.Add("m_WrapMode", (int)WrapMode);
			node.Add("m_PlayAutomatically", PlayAutomatically);
			node.Add("m_AnimatePhysics", AnimatePhysics);
			node.Add("m_CullingType", (int)CullingType);
			return node;
		}
		
		public IReadOnlyList<Tuple<string, PPtr<AnimationClip>>> AnimationsPaired => m_animationsPaired;
		public IReadOnlyList<PPtr<AnimationClip>> Animations => m_animations;
		public WrapMode WrapMode { get; private set; }
		public bool PlayAutomatically { get; private set; }
		/// <summary>
		/// PlayFixedFrameRate previously
		/// </summary>
		public bool AnimatePhysics { get; private set; }
		public bool AnimateOnlyIfVisible { get; private set; }
		public AnimationCullingType CullingType { get; private set; }

		/// <summary>
		/// Animation previously
		/// </summary>
		public PPtr<AnimationClip> DefaultAnimation;
		public AABB UserAABB;

		private Tuple<string, PPtr<AnimationClip>>[] m_animationsPaired;
		private PPtr<AnimationClip>[] m_animations;
	}
}
