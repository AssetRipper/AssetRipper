using AssetRipper.Core.Classes.AnimationClip.Clip;
using AssetRipper.Core.Classes.AnimationClip.Curves;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Converters.AnimationClip;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class AnimationClip : Motion
	{
		private struct AnimationCurves
		{
			public IEnumerable<QuaternionCurve> RotationCurves { get; set; }
			public IEnumerable<CompressedAnimationCurve> CompressedRotationCurves { get; set; }
			public IEnumerable<Vector3Curve> EulerCurves { get; set; }
			public IEnumerable<Vector3Curve> PositionCurves { get; set; }
			public IEnumerable<Vector3Curve> ScaleCurves { get; set; }
			public IEnumerable<FloatCurve> FloatCurves { get; set; }
			public IEnumerable<PPtrCurve> PPtrCurves { get; set; }
		}

		public AnimationClip(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2))
			{
				return 6;
			}
			if (version.IsGreaterEqual(5))
			{
				return 5;
			}
			if (version.IsGreaterEqual(4, 3))
			{
				return 4;
			}
			if (version.IsGreaterEqual(2, 6))
			{
				return 3;
			}
			// min version is 2nd
			return 2;
		}

		/// <summary>
		/// 4.x.x
		/// </summary>
		public static bool HasAnimationType(UnityVersion version) => version.IsEqual(4);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLegacy(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasUseHightQualityCurve(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasEulerCurves(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasPPtrCurves(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.0.0 and greater and Release
		/// </summary>
		public static bool HasMuscleClip(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(4) && flags.IsRelease();
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasClipBindingConstant(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// <para>5.0.0 and greater and Not Release</para>
		/// <para>2018.3 and greater</para>
		/// </summary>
		public static bool HasHasGenericRootTransform(UnityVersion version, TransferInstructionFlags flags)
		{
			if (flags.IsRelease())
			{
				return version.IsGreaterEqual(2018, 3);
			}
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// <para>5.0.0f1 and greater and Not Release</para>
		/// <para>2018.3 and greater</para>
		/// </summary>
		public static bool HasHasMotionFloatCurves(UnityVersion version, TransferInstructionFlags flags)
		{
			if (flags.IsRelease())
			{
				return version.IsGreaterEqual(2018, 3);
			}
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2017);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasAnimationType(reader.Version))
			{
				AnimationType = (AnimationType)reader.ReadInt32();
			}
			if (HasLegacy(reader.Version))
			{
				Legacy = reader.ReadBoolean();
			}

			Compressed = reader.ReadBoolean();
			if (HasUseHightQualityCurve(reader.Version))
			{
				UseHightQualityCurve = reader.ReadBoolean();
			}
			reader.AlignStream();

			RotationCurves = reader.ReadAssetArray<QuaternionCurve>();
			CompressedRotationCurves = reader.ReadAssetArray<CompressedAnimationCurve>();
			if (HasEulerCurves(reader.Version))
			{
				EulerCurves = reader.ReadAssetArray<Vector3Curve>();
			}
			PositionCurves = reader.ReadAssetArray<Vector3Curve>();
			ScaleCurves = reader.ReadAssetArray<Vector3Curve>();
			FloatCurves = reader.ReadAssetArray<FloatCurve>();
			if (HasPPtrCurves(reader.Version))
			{
				PPtrCurves = reader.ReadAssetArray<PPtrCurve>();
			}

			SampleRate = reader.ReadSingle();

			WrapMode = (WrapMode)reader.ReadInt32();
			Bounds.Read(reader);
			if (HasMuscleClip(reader.Version, reader.Flags))
			{
				MuscleClipSize = reader.ReadUInt32();
				MuscleClip = new ClipMuscleConstant();
				MuscleClip.Read(reader);
			}
			if (HasClipBindingConstant(reader.Version))
			{
				ClipBindingConstant.Read(reader);
			}

			if (HasHasGenericRootTransform(reader.Version, reader.Flags))
			{
				HasGenericRootTransform = reader.ReadBoolean();
			}
			if (HasHasMotionFloatCurves(reader.Version, reader.Flags))
			{
				HasMotionFloatCurves = reader.ReadBoolean();
			}
			if (HasHasGenericRootTransform(reader.Version, reader.Flags))
			{
				reader.AlignStream();
			}

			Events = reader.ReadAssetArray<AnimationEvent>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(FloatCurves, FloatCurvesName))
			{
				yield return asset;
			}
			if (HasPPtrCurves(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(PPtrCurves, PPtrCurvesName))
				{
					yield return asset;
				}
			}
			if (HasClipBindingConstant(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(ClipBindingConstant, ClipBindingConstantName))
				{
					yield return asset;
				}
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Events, EventsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(LegacyName, GetLegacy(container.Version));
			node.Add(CompressedName, Compressed);
			node.Add(UseHighQualityCurveName, UseHightQualityCurve);

			AnimationCurves curves = GetAnimationCurves(container.Version, container.Flags);
			node.Add(RotationCurvesName, curves.RotationCurves.ExportYAML(container));
			node.Add(CompressedRotationCurvesName, curves.CompressedRotationCurves.ExportYAML(container));
			node.Add(EulerCurvesName, curves.EulerCurves.ExportYAML(container));
			node.Add(PositionCurvesName, curves.PositionCurves.ExportYAML(container));
			node.Add(ScaleCurvesName, curves.ScaleCurves.ExportYAML(container));
			node.Add(FloatCurvesName, curves.FloatCurves.ExportYAML(container));
			node.Add(PPtrCurvesName, curves.PPtrCurves.ExportYAML(container));

			node.Add(SampleRateName, SampleRate);
			node.Add(WrapModeName, (int)WrapMode);
			node.Add(BoundsName, Bounds.ExportYAML(container));
			node.Add(ClipBindingConstantName, GetClipBindingConstant(container.Version).ExportYAML(container));
			node.Add(AnimationClipSettingsName, GetAnimationClipSettings(container.Version, container.Flags).ExportYAML(container));
			node.Add(EditorCurvesName, Array.Empty<FloatCurve>().ExportYAML(container));
			node.Add(EulerEditorCurvesName, Array.Empty<FloatCurve>().ExportYAML(container));
			node.Add(HasGenericRootTransformName, HasGenericRootTransform);
			node.Add(HasMotionFloatCurvesName, HasMotionFloatCurves);
			node.Add(GenerateMotionCurvesName, false);
			node.Add(EventsName, Events.ExportYAML(container));

			return node;
		}

		private AnimationCurves ExportGenericData()
		{
			AnimationClipConverter converter = AnimationClipConverter.Process(this);
			return new AnimationCurves()
			{
				RotationCurves = converter.Rotations.Union(RotationCurves),
				CompressedRotationCurves = CompressedRotationCurves,
				EulerCurves = converter.Eulers.Union(GetEulerCurves(SerializedFile.Version)),
				PositionCurves = converter.Translations.Union(PositionCurves),
				ScaleCurves = converter.Scales.Union(ScaleCurves),
				FloatCurves = converter.Floats.Union(FloatCurves),
				PPtrCurves = converter.PPtrs.Union(GetPPtrCurves(SerializedFile.Version)),
			};
		}

		public IReadOnlyDictionary<uint, string> FindTOS()
		{
			Dictionary<uint, string> tos = new Dictionary<uint, string>() { { 0, string.Empty } };

			foreach (IUnityObjectBase asset in SerializedFile.Collection.FetchAssetsOfType(ClassIDType.Avatar))
			{
				Avatar.Avatar avatar = (Avatar.Avatar)asset;
				if (AddAvatarTOS(avatar, tos))
				{
					return tos;
				}
			}

			foreach (IUnityObjectBase asset in SerializedFile.Collection.FetchAssetsOfType(ClassIDType.Animator))
			{
				Animator.Animator animator = (Animator.Animator)asset;
				if (IsAnimatorContainsClip(animator))
				{
					if (AddAnimatorTOS(animator, tos))
					{
						return tos;
					}
				}
			}

			foreach (IUnityObjectBase asset in SerializedFile.Collection.FetchAssetsOfType(ClassIDType.Animation))
			{
				Animation.Animation animation = (Animation.Animation)asset;
				if (IsAnimationContainsClip(animation))
				{
					if (AddAnimationTOS(animation, tos))
					{
						return tos;
					}
				}
			}

			return tos;
		}

		public IEnumerable<GameObject.GameObject> FindRoots()
		{
			foreach (IUnityObjectBase asset in SerializedFile.Collection.FetchAssets())
			{
				switch (asset.ClassID)
				{
					case ClassIDType.Animator:
						Animator.Animator animator = (Animator.Animator)asset;
						if (IsAnimatorContainsClip(animator))
						{
							yield return animator.GameObject.GetAsset(animator.SerializedFile);
						}
						break;

					case ClassIDType.Animation:
						Animation.Animation animation = (Animation.Animation)asset;
						if (IsAnimationContainsClip(animation))
						{
							yield return animation.GameObject.GetAsset(animation.SerializedFile);
						}
						break;
				}
			}

			yield break;
		}

		private bool IsAnimatorContainsClip(Animator.Animator animator)
		{
			RuntimeAnimatorController runetime = animator.Controller.FindAsset(animator.SerializedFile);
			if (runetime == null)
			{
				return false;
			}
			else
			{
				return runetime.IsContainsAnimationClip(this);
			}
		}

		private bool IsAnimationContainsClip(Animation.Animation animation)
		{
			return animation.IsContainsAnimationClip(this);
		}

		private bool AddAvatarTOS(Avatar.Avatar avatar, Dictionary<uint, string> tos)
		{
			return AddTOS(avatar.TOS, tos);
		}

		private bool AddAnimatorTOS(Animator.Animator animator, Dictionary<uint, string> tos)
		{
			Avatar.Avatar avatar = animator.Avatar.FindAsset(animator.SerializedFile);
			if (avatar != null)
			{
				if (AddAvatarTOS(avatar, tos))
				{
					return true;
				}
			}

			IReadOnlyDictionary<uint, string> animatorTOS = animator.BuildTOS();
			return AddTOS(animatorTOS, tos);
		}

		private bool AddAnimationTOS(Animation.Animation animation, Dictionary<uint, string> tos)
		{
			GameObject.GameObject go = animation.GameObject.GetAsset(animation.SerializedFile);
			IReadOnlyDictionary<uint, string> animationTOS = go.BuildTOS();
			return AddTOS(animationTOS, tos);
		}

		private bool AddTOS(IReadOnlyDictionary<uint, string> src, Dictionary<uint, string> dest)
		{
			int tosCount = ClipBindingConstant.GenericBindings.Length;
			for (int i = 0; i < tosCount; i++)
			{
				GenericBinding.GenericBinding binding = ClipBindingConstant.GenericBindings[i];
				if (src.TryGetValue(binding.Path, out string path))
				{
					dest[binding.Path] = path;
					if (dest.Count == tosCount)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsExportGenericData(UnityVersion version, TransferInstructionFlags flags)
		{
			if (HasLegacy(version))
			{
				if (HasMuscleClip(version, flags))
				{
					return MuscleClip.Clip.IsSet(version);
				}
				return false;
			}
			if (HasAnimationType(version))
			{
				if (!HasClipBindingConstant(version))
				{
#warning TODO:
					return false;
				}
				if (HasMuscleClip(version, flags))
				{
					if (AnimationType != AnimationType.Legacy)
					{
						return MuscleClip.Clip.IsSet(version);
					}
				}
			}
			return false;
		}

		private bool GetLegacy(UnityVersion version)
		{
			if (HasLegacy(version))
			{
				return Legacy;
			}
			return AnimationType == AnimationType.Legacy;
		}

		private AnimationCurves GetAnimationCurves(UnityVersion version, TransferInstructionFlags flags)
		{
			if (IsExportGenericData(version, flags))
			{
				return ExportGenericData();
			}
			else
			{
				return new AnimationCurves()
				{
					RotationCurves = RotationCurves,
					CompressedRotationCurves = CompressedRotationCurves,
					EulerCurves = GetEulerCurves(SerializedFile.Version),
					PositionCurves = PositionCurves,
					ScaleCurves = ScaleCurves,
					FloatCurves = FloatCurves,
					PPtrCurves = GetPPtrCurves(SerializedFile.Version),
				};
			}
		}

		private IReadOnlyList<Vector3Curve> GetEulerCurves(UnityVersion version)
		{
			return HasEulerCurves(version) ? EulerCurves : Array.Empty<Vector3Curve>();
		}
		private IReadOnlyList<PPtrCurve> GetPPtrCurves(UnityVersion version)
		{
			return HasPPtrCurves(version) ? PPtrCurves : Array.Empty<PPtrCurve>();
		}
		private AnimationClipBindingConstant GetClipBindingConstant(UnityVersion version)
		{
			return HasClipBindingConstant(version) ? ClipBindingConstant : new AnimationClipBindingConstant(true);
		}
		private AnimationClipSettings GetAnimationClipSettings(UnityVersion version, TransferInstructionFlags flags)
		{
			return HasMuscleClip(version, flags) ? new AnimationClipSettings(MuscleClip) : new AnimationClipSettings(true);
		}

		public override string ExportExtension => "anim";

		public Dictionary<int, PPtr<BaseAnimationTrack>> ClassIDToTrack { get; set; }
		public ChildTrack[] ChildTracks { get; set; }
		public AnimationType AnimationType { get; set; }
		public bool Legacy { get; set; }
		public bool Compressed { get; set; }
		public bool UseHightQualityCurve { get; set; }
		public QuaternionCurve[] RotationCurves { get; set; }
		public CompressedAnimationCurve[] CompressedRotationCurves { get; set; }
		public Vector3Curve[] EulerCurves { get; set; }
		public Vector3Curve[] PositionCurves { get; set; }
		public Vector3Curve[] ScaleCurves { get; set; }
		public FloatCurve[] FloatCurves { get; set; }
		public PPtrCurve[] PPtrCurves { get; set; }
		public float SampleRate { get; set; }
		public WrapMode WrapMode { get; set; }
		public uint MuscleClipSize { get; set; }
		public ClipMuscleConstant MuscleClip { get; set; }
		public bool HasGenericRootTransform { get; set; }
		public bool HasMotionFloatCurves { get; set; }
		public AnimationEvent[] Events { get; set; }

		public const string ClassIDToTrackName = "m_ClassIDToTrack";
		public const string ChildTracksName = "m_ChildTracks";
		public const string LegacyName = "m_Legacy";
		public const string CompressedName = "m_Compressed";
		public const string UseHighQualityCurveName = "m_UseHighQualityCurve";
		public const string RotationCurvesName = "m_RotationCurves";
		public const string CompressedRotationCurvesName = "m_CompressedRotationCurves";
		public const string EulerCurvesName = "m_EulerCurves";
		public const string PositionCurvesName = "m_PositionCurves";
		public const string ScaleCurvesName = "m_ScaleCurves";
		public const string FloatCurvesName = "m_FloatCurves";
		public const string PPtrCurvesName = "m_PPtrCurves";
		public const string SampleRateName = "m_SampleRate";
		public const string WrapModeName = "m_WrapMode";
		public const string BoundsName = "m_Bounds";
		public const string ClipBindingConstantName = "m_ClipBindingConstant";
		public const string AnimationClipSettingsName = "m_AnimationClipSettings";
		public const string EditorCurvesName = "m_EditorCurves";
		public const string EulerEditorCurvesName = "m_EulerEditorCurves";
		public const string HasGenericRootTransformName = "m_HasGenericRootTransform";
		public const string HasMotionFloatCurvesName = "m_HasMotionFloatCurves";
		public const string GenerateMotionCurvesName = "m_GenerateMotionCurves";
		public const string IsEmptyName = "m_IsEmpty";
		public const string EventsName = "m_Events";

		public AABB Bounds = new AABB();
		public AnimationClipBindingConstant ClipBindingConstant = new();
	}
}
