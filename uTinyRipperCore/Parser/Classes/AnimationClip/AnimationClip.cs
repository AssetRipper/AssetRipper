using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
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

		public AnimationClip(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2))
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
		/// Less than 2.0.0
		/// </summary>
		public static bool HasClassIDToTrack(Version version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(2, 6))
			{
				return false;
			}
			if (version.IsGreaterEqual(2))
			{
				return !flags.IsRelease();
			}
			return true;
		}
		/// <summary>
		/// 4.x.x
		/// </summary>
		public static bool HasAnimationType(Version version) => version.IsEqual(4);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLegacy(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasCompressed(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasUseHightQualityCurve(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasCurves(Version version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasCompressedRotationCurves(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasEulerCurves(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasPPtrCurves(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasSampleRate(Version version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasWrapMode(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasBounds(Version version) => version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 4.0.0 and greater and Release
		/// </summary>
		public static bool HasMuscleClip(Version version, TransferInstructionFlags flags) => version.IsGreaterEqual(4) && flags.IsRelease();
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasClipBindingConstant(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.0.0 and Not Release
		/// </summary>
		public static bool HasAnimationClipSettings(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(4);
		/// <summary>
		/// 2.6.0 and greater and Not Release
		/// </summary>
		public static bool HasEditorCurves(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2, 6);
		/// <summary>
		/// <para>5.0.0 and greater and Not Release</para>
		/// <para>2018.3 and greater</para>
		/// </summary>
		public static bool HasHasGenericRootTransform(Version version, TransferInstructionFlags flags)
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
		public static bool HasHasMotionFloatCurves(Version version, TransferInstructionFlags flags)
		{
			if (flags.IsRelease())
			{
				return version.IsGreaterEqual(2018, 3);
			}
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.0 to 2018.3 exclusive and Not Release
		/// </summary>
		public static bool HasGenerateMotionCurves(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5) && version.IsLess(2018, 3);
		}
		/// <summary>
		/// 5.5.0 to 5.6.0b9 and Not Release
		/// </summary>
		public static bool HasIsEmpty(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 5) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 9);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool HasEvents(Version version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive and Not Release
		/// </summary>
		public static bool HasRuntimeEvents(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlignCompressed(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(2017);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasClassIDToTrack(reader.Version, reader.Flags))
			{
				ClassIDToTrack = new Dictionary<int, PPtr<BaseAnimationTrack>>();
				ClassIDToTrack.Read(reader);
				ChildTracks = reader.ReadAssetArray<ChildTrack>();
			}

			if (HasAnimationType(reader.Version))
			{
				AnimationType = (AnimationType)reader.ReadInt32();
			}
			if (HasLegacy(reader.Version))
			{
				Legacy = reader.ReadBoolean();
			}

			if (HasCompressed(reader.Version))
			{
				Compressed = reader.ReadBoolean();
			}
			if (HasUseHightQualityCurve(reader.Version))
			{
				UseHightQualityCurve = reader.ReadBoolean();
			}
			if (IsAlignCompressed(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasCurves(reader.Version))
			{
				RotationCurves = reader.ReadAssetArray<QuaternionCurve>();
			}
			if (HasCompressedRotationCurves(reader.Version))
			{
				CompressedRotationCurves = reader.ReadAssetArray<CompressedAnimationCurve>();
			}
			if (HasEulerCurves(reader.Version))
			{
				EulerCurves = reader.ReadAssetArray<Vector3Curve>();
			}
			if (HasCurves(reader.Version))
			{
				PositionCurves = reader.ReadAssetArray<Vector3Curve>();
				ScaleCurves = reader.ReadAssetArray<Vector3Curve>();
				FloatCurves = reader.ReadAssetArray<FloatCurve>();
			}
			if (HasPPtrCurves(reader.Version))
			{
				PPtrCurves = reader.ReadAssetArray<PPtrCurve>();
			}

			if (HasSampleRate(reader.Version))
			{
				SampleRate = reader.ReadSingle();
			}

			if (HasWrapMode(reader.Version))
			{
				WrapMode = (WrapMode)reader.ReadInt32();
			}
			if (HasBounds(reader.Version))
			{
				Bounds.Read(reader);
			}
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
#if UNIVERSAL
			if (HasAnimationClipSettings(reader.Version, reader.Flags))
			{
				AnimationClipSettings = new AnimationClipSettings();
				AnimationClipSettings.Read(reader);
			}
			if (HasEditorCurves(reader.Version, reader.Flags))
			{
				EditorCurves = reader.ReadAssetArray<FloatCurve>();
				EulerEditorCurves = reader.ReadAssetArray<FloatCurve>();
			}
#endif

			if (HasHasGenericRootTransform(reader.Version, reader.Flags))
			{
				HasGenericRootTransform = reader.ReadBoolean();
			}
			if (HasHasMotionFloatCurves(reader.Version, reader.Flags))
			{
				HasMotionFloatCurves = reader.ReadBoolean();
			}
#if UNIVERSAL
			if (HasGenerateMotionCurves(reader.Version, reader.Flags))
			{
				GenerateMotionCurves = reader.ReadBoolean();
			}
			if (HasIsEmpty(reader.Version, reader.Flags))
			{
				IsEmpty = reader.ReadBoolean();
			}
#endif
			if (HasHasGenericRootTransform(reader.Version, reader.Flags))
			{
				reader.AlignStream();
			}

			if (HasEvents(reader.Version))
			{
				Events = reader.ReadAssetArray<AnimationEvent>();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

#if UNIVERSAL
			if (HasRuntimeEvents(reader.Version, reader.Flags))
			{
				RunetimeEvents = reader.ReadAssetArray<AnimationEvent>();
			}
#endif
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasClassIDToTrack(context.Version, context.Flags))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies((IEnumerable<PPtr<BaseAnimationTrack>>)ClassIDToTrack.Values, ClassIDToTrackName))
				{
					yield return asset;
				}
				foreach (PPtr<Object> asset in context.FetchDependencies(ChildTracks, ChildTracksName))
				{
					yield return asset;
				}
			}
			if (HasCurves(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(FloatCurves, FloatCurvesName))
				{
					yield return asset;
				}
			}
			if (HasPPtrCurves(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(PPtrCurves, PPtrCurvesName))
				{
					yield return asset;
				}
			}
			if (HasClipBindingConstant(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(ClipBindingConstant, ClipBindingConstantName))
				{
					yield return asset;
				}
			}
#if UNIVERSAL
			if (HasAnimationClipSettings(context.Version, context.Flags))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(AnimationClipSettings, AnimationClipSettingsName))
				{
					yield return asset;
				}
			}
			if (HasEditorCurves(context.Version, context.Flags))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(EditorCurves, EditorCurvesName))
				{
					yield return asset;
				}
				foreach (PPtr<Object> asset in context.FetchDependencies(EulerEditorCurves, EulerEditorCurvesName))
				{
					yield return asset;
				}
			}
#endif
			if (HasEvents(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(Events, EventsName))
				{
					yield return asset;
				}
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
			node.Add(EditorCurvesName, GetEditorCurves(container.Version, container.Flags).ExportYAML(container));
			node.Add(EulerEditorCurvesName, GetEulerEditorCurves(container.Version, container.Flags).ExportYAML(container));
			node.Add(HasGenericRootTransformName, HasGenericRootTransform);
			node.Add(HasMotionFloatCurvesName, HasMotionFloatCurves);
			node.Add(GenerateMotionCurvesName, GetGenerateMotionCurves(container.Version, container.Flags));
			node.Add(EventsName, GetEvents(container.Version).ExportYAML(container));

			return node;
		}

		private AnimationCurves ExportGenericData()
		{
			AnimationClipConverter converter = AnimationClipConverter.Process(this);
			return new AnimationCurves()
			{
				RotationCurves = converter.Rotations.Union(GetRotationCurves(File.Version)),
				CompressedRotationCurves = GetCompressedRotationCurves(File.Version),
				EulerCurves = converter.Eulers.Union(GetEulerCurves(File.Version)),
				PositionCurves = converter.Translations.Union(GetPositionCurves(File.Version)),
				ScaleCurves = converter.Scales.Union(GetScaleCurves(File.Version)),
				FloatCurves = converter.Floats.Union(GetFloatCurves(File.Version)),
				PPtrCurves = converter.PPtrs.Union(GetPPtrCurves(File.Version)),
			};
		}

		public IReadOnlyDictionary<uint, string> FindTOS()
		{
			Dictionary<uint, string> tos = new Dictionary<uint, string>() { { 0, string.Empty }  };
			foreach (Object asset in File.Collection.FetchAssets())
			{
				switch (asset.ClassID)
				{
					case ClassIDType.Avatar:
						{
							Avatar avatar = (Avatar)asset;
							if (AddAvatarTOS(avatar, tos))
							{
								return tos;
							}
						}
						break;

					case ClassIDType.Animator:
						Animator animator = (Animator)asset;
						if (IsAnimatorContainsClip(animator))
						{
							if (AddAnimatorTOS(animator, tos))
							{
								return tos;
							}
						}
						break;

					case ClassIDType.Animation:
						Animation animation = (Animation)asset;
						if (IsAnimationContainsClip(animation))
						{
							if (AddAnimationTOS(animation, tos))
							{
								return tos;
							}
						}
						break;
				}
			}
			return tos;
		}

		public IEnumerable<GameObject> FindRoots()
		{
			foreach (Object asset in File.Collection.FetchAssets())
			{
				switch (asset.ClassID)
				{
					case ClassIDType.Animator:
						Animator animator = (Animator)asset;
						if (IsAnimatorContainsClip(animator))
						{
							yield return animator.GameObject.GetAsset(animator.File);
						}
						break;

					case ClassIDType.Animation:
						Animation animation = (Animation)asset;
						if (IsAnimationContainsClip(animation))
						{
							yield return animation.GameObject.GetAsset(animation.File);
						}
						break;
				}
			}

			yield break;
		}

		private bool IsAnimatorContainsClip(Animator animator)
		{
			RuntimeAnimatorController runetime = animator.Controller.FindAsset(animator.File);
			if (runetime == null)
			{
				return false;
			}
			else
			{
				return runetime.IsContainsAnimationClip(this);
			}
		}

		private bool IsAnimationContainsClip(Animation animation)
		{
			return animation.IsContainsAnimationClip(this);
		}

		private bool AddAvatarTOS(Avatar avatar, Dictionary<uint, string> tos)
		{
			return AddTOS(avatar.TOS, tos);
		}

		private bool AddAnimatorTOS(Animator animator, Dictionary<uint, string> tos)
		{
			Avatar avatar = animator.Avatar.FindAsset(animator.File);
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

		private bool AddAnimationTOS(Animation animation, Dictionary<uint, string> tos)
		{
			GameObject go = animation.GameObject.GetAsset(animation.File);
			IReadOnlyDictionary<uint, string> animationTOS = go.BuildTOS();
			return AddTOS(animationTOS, tos);
		}

		private bool AddTOS(IReadOnlyDictionary<uint, string> src, Dictionary<uint, string> dest)
		{
			int tosCount = ClipBindingConstant.GenericBindings.Length;
			for (int i = 0; i < tosCount; i++)
			{
				ref GenericBinding binding = ref ClipBindingConstant.GenericBindings[i];
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

		private bool IsExportGenericData(Version version, TransferInstructionFlags flags)
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

		private bool GetLegacy(Version version)
		{
			if (HasLegacy(version))
			{
				return Legacy;
			}
			return AnimationType == AnimationType.Legacy;
		}

		private AnimationCurves GetAnimationCurves(Version version, TransferInstructionFlags flags)
		{
			if (IsExportGenericData(version, flags))
			{
				return ExportGenericData();
			}
			else
			{
				return new AnimationCurves()
				{
					RotationCurves = GetRotationCurves(File.Version),
					CompressedRotationCurves = GetCompressedRotationCurves(File.Version),
					EulerCurves = GetEulerCurves(File.Version),
					PositionCurves = GetPositionCurves(File.Version),
					ScaleCurves = GetScaleCurves(File.Version),
					FloatCurves = GetFloatCurves(File.Version),
					PPtrCurves = GetPPtrCurves(File.Version),
				};
			}
		}

		private IReadOnlyList<QuaternionCurve> GetRotationCurves(Version version)
		{
			return HasCurves(version) ? RotationCurves : Array.Empty<QuaternionCurve>();
		}
		private IReadOnlyList<CompressedAnimationCurve> GetCompressedRotationCurves(Version version)
		{
			return HasCompressedRotationCurves(version) ? CompressedRotationCurves : Array.Empty<CompressedAnimationCurve>();
		}
		private IReadOnlyList<Vector3Curve> GetEulerCurves(Version version)
		{
			return HasEulerCurves(version) ? EulerCurves : Array.Empty<Vector3Curve>();
		}
		private IReadOnlyList<Vector3Curve> GetPositionCurves(Version version)
		{
			return HasCurves(version) ? PositionCurves : Array.Empty<Vector3Curve>();
		}
		private IReadOnlyList<Vector3Curve> GetScaleCurves(Version version)
		{
			return HasCurves(version) ? ScaleCurves : Array.Empty<Vector3Curve>();
		}
		private IReadOnlyList<FloatCurve> GetFloatCurves(Version version)
		{
			return HasCurves(version) ? FloatCurves : Array.Empty<FloatCurve>();
		}
		private IReadOnlyList<PPtrCurve> GetPPtrCurves(Version version)
		{
			return HasPPtrCurves(version) ? PPtrCurves : Array.Empty<PPtrCurve>();
		}
		private AnimationClipBindingConstant GetClipBindingConstant(Version version)
		{
			return HasClipBindingConstant(version) ? ClipBindingConstant : new AnimationClipBindingConstant(true);
		}
		private AnimationClipSettings GetAnimationClipSettings(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasAnimationClipSettings(version, flags))
			{
				return AnimationClipSettings;
			}
#endif
			return HasMuscleClip(version, flags) ? new AnimationClipSettings(MuscleClip) : new AnimationClipSettings(true);
		}
		private IReadOnlyList<FloatCurve> GetEditorCurves(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorCurves(version, flags))
			{
				return EditorCurves;
			}
#endif
			return Array.Empty<FloatCurve>();
		}
		private IReadOnlyList<FloatCurve> GetEulerEditorCurves(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorCurves(version, flags))
			{
				return EulerEditorCurves;
			}
#endif
			return Array.Empty<FloatCurve>();
		}
		private bool GetGenerateMotionCurves(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasGenerateMotionCurves(version, flags))
			{
				return GenerateMotionCurves;
			}
#endif
			return false;
		}
		private IReadOnlyList<AnimationEvent> GetEvents(Version version)
		{
			return HasEvents(version) ? Events : Array.Empty<AnimationEvent>();
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
#if UNIVERSAL
		public AnimationClipSettings AnimationClipSettings { get; set; }
		public FloatCurve[] EditorCurves { get; set; }
		public FloatCurve[] EulerEditorCurves { get; set; }
#endif
		public bool HasGenericRootTransform { get; set; }
		public bool HasMotionFloatCurves { get; set; }
#if UNIVERSAL
		public bool GenerateMotionCurves { get; set; }
		public bool IsEmpty { get; set; }
#endif
		public AnimationEvent[] Events { get; set; }
#if UNIVERSAL
		public AnimationEvent[] RunetimeEvents { get; set; }
#endif

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

		public AABB Bounds;
		public AnimationClipBindingConstant ClipBindingConstant;
	}
}
