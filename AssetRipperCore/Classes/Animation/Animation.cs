using AssetRipper.Core.Classes.AnimationClip;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Converters;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Animation
{
	public sealed class Animation : Behaviour
	{
		public Animation(LayoutInfo layout) : base(layout)
		{
			Animations = Array.Empty<PPtr<AnimationClip.AnimationClip>>();
			PlayAutomatically = true;
			Enabled = true;
		}

		public Animation(AssetInfo assetInfo) : base(assetInfo) { }

		public override IUnityObjectBase ConvertLegacy(IExportContainer container)
		{
			return AnimationConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DefaultAnimation.Read(reader);
			Animations = reader.ReadAssetArray<PPtr<AnimationClip.AnimationClip>>();

			WrapMode = (WrapMode)reader.ReadInt32();
			PlayAutomatically = reader.ReadBoolean();
			AnimatePhysics = reader.ReadBoolean();
			if (HasAnimateOnlyIfVisible(reader.Version))
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

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			DefaultAnimation.Write(writer);
			Animations.Write(writer);

			writer.Write((int)WrapMode);
			writer.Write(PlayAutomatically);
			writer.Write(AnimatePhysics);
			if (HasAnimateOnlyIfVisible(writer.Version))
			{
				writer.Write(AnimateOnlyIfVisible);
			}
			if (IsAlign(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasCullingType(writer.Version))
			{
				writer.Write((int)CullingType);
			}
			if (HasUserAABB(writer.Version))
			{
				UserAABB.Write(writer);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(DefaultAnimation, AnimationName);

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Animations, AnimationsName))
			{
				yield return asset;
			}
		}

		public bool IsContainsAnimationClip(AnimationClip.AnimationClip clip)
		{
			foreach (PPtr<AnimationClip.AnimationClip> clipPtr in Animations)
			{
				if (clipPtr.IsAsset(SerializedFile, clip))
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
			if (HasAnimateOnlyIfVisible(container.ExportVersion))
			{
				node.Add(AnimateOnlyIfVisibleName, AnimateOnlyIfVisible);
			}
			if (HasCullingType(container.ExportVersion))
			{
				node.Add(CullingTypeName, (int)CullingType);
			}
			if (HasUserAABB(container.ExportVersion))
			{
				node.Add(UserAABBName, UserAABB.ExportYAML(container));
			}
			return node;
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(3, 4))
			{
				// AnimateOnlyIfVisible has been replaced by CullingType
				return 3;
			}
			else if (version.IsGreaterEqual(1, 5))
			{
				// PlayFixedFrameRate has been renamed to AnimatePhysics
				return 2;
			}
			else
			{
				return 1;
			}
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasCullingTypeInvariant(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasCullingType(UnityVersion version) => version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 2.6.0 to 3.4.0 exclusive
		/// </summary>
		public static bool HasAnimateOnlyIfVisible(UnityVersion version) => version.IsGreaterEqual(2, 6) && version.IsLess(3, 4);
		/// <summary>
		/// 3.4.0 to 4.3.0 exclusive
		/// </summary>
		public static bool HasUserAABB(UnityVersion version) => version.IsGreaterEqual(3, 4) && version.IsLess(4, 3);
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(3, 2);

		public PPtr<AnimationClip.AnimationClip>[] Animations { get; set; }
		public WrapMode WrapMode { get; set; }
		public bool PlayAutomatically { get; set; }
		public bool AnimatePhysics { get; set; }
		public bool PlayFixedFrameRate
		{
			get => AnimatePhysics;
			set => AnimatePhysics = value;
		}
		public bool AnimateOnlyIfVisible
		{
			get => CullingType != AnimationCullingType.AlwaysAnimate;
			set => CullingType = value ? AnimationCullingType.BasedOnRenderers : AnimationCullingType.AlwaysAnimate;
		}
		public AnimationCullingType CullingType { get; set; }

		public PPtr<AnimationClip.AnimationClip> DefaultAnimation = new();
		public AABB UserAABB = new AABB();

		public const string AnimationName = "m_Animation";
		public const string AnimationsName = "m_Animations";
		public const string WrapModeName = "m_WrapMode";
		public const string PlayAutomaticallyName = "m_PlayAutomatically";
		public const string AnimatePhysicsName = "m_AnimatePhysics";
		public const string CullingTypeName = "m_CullingType";
		public const string AnimateOnlyIfVisibleName = "m_AnimateOnlyIfVisible";
		public const string UserAABBName = "m_UserAABB";
	}
}
