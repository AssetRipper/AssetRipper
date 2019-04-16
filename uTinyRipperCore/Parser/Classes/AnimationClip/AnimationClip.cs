using System.Collections.Generic;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class AnimationClip : Motion
	{
		public AnimationClip(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

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

		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool IsReadClassIDToTrack(Version version, TransferInstructionFlags flags)
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
		/// 4.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadAnimationType(Version version)
		{
			return version.IsGreaterEqual(4) && version.IsLess(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLegacy(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadCompressed(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadUseHightQualityCurve(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool IsReadCurves(Version version)
		{
			return version.IsGreaterEqual(1, 5);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadCompressedRotationCurves(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadEulerCurves(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadPPtrCurves(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool IsReadSampleRate(Version version)
		{
			return version.IsGreaterEqual(1, 5);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadWrapMode(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadBounds(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 4.0.0 and greater and Release
		/// </summary>
		public static bool IsReadMuscleClip(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(4) && flags.IsRelease();
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadClipBindingConstant(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.0.0 and Not Release
		/// </summary>
		public static bool IsReadAnimationClipSettings(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2.6.0 and greater and Not Release
		/// </summary>
		public static bool IsReadEditorCurves(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// <para>5.0.0 and greater and Not Release</para>
		/// <para>2018.3 and greater</para>
		/// </summary>
		public static bool IsReadHasGenericRootTransform(Version version, TransferInstructionFlags flags)
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
		public static bool IsReadHasMotionFloatCurves(Version version, TransferInstructionFlags flags)
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
		public static bool IsReadGenerateMotionCurves(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5) && version.IsLess(2018, 3);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadEvents(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive and Not Release
		/// </summary>
		public static bool IsReadRuntimeEvents(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlignCompressed(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		private static int GetSerializedVersion(Version version)
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadClassIDToTrack(reader.Version, reader.Flags))
			{
				m_classIDToTrack = new Dictionary<int, PPtr<BaseAnimationTrack>>();
				m_classIDToTrack.Read(reader);
				m_childTracks = reader.ReadAssetArray<ChildTrack>();
			}

			if (IsReadAnimationType(reader.Version))
			{
				AnimationType = (AnimationType)reader.ReadInt32();
			}
			if (IsReadLegacy(reader.Version))
			{
				Legacy = reader.ReadBoolean();
			}

			if (IsReadCompressed(reader.Version))
			{
				Compressed = reader.ReadBoolean();
			}
			if (IsReadUseHightQualityCurve(reader.Version))
			{
				UseHightQualityCurve = reader.ReadBoolean();
			}
			if (IsAlignCompressed(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadCurves(reader.Version))
			{
				m_rotationCurves = reader.ReadAssetArray<QuaternionCurve>();
			}
			if (IsReadCompressedRotationCurves(reader.Version))
			{
				m_compressedRotationCurves = reader.ReadAssetArray<CompressedAnimationCurve>();
			}
			if (IsReadEulerCurves(reader.Version))
			{
				m_eulerCurves = reader.ReadAssetArray<Vector3Curve>();
			}
			if (IsReadCurves(reader.Version))
			{
				m_positionCurves = reader.ReadAssetArray<Vector3Curve>();
				m_scaleCurves = reader.ReadAssetArray<Vector3Curve>();
				m_floatCurves = reader.ReadAssetArray<FloatCurve>();
			}
			if (IsReadPPtrCurves(reader.Version))
			{
				m_PPtrCurves = reader.ReadAssetArray<PPtrCurve>();
			}

			if (IsReadSampleRate(reader.Version))
			{
				SampleRate = reader.ReadSingle();
			}

			if (IsReadWrapMode(reader.Version))
			{
				WrapMode = (WrapMode)reader.ReadInt32();
			}
			if (IsReadBounds(reader.Version))
			{
				Bounds.Read(reader);
			}
			if (IsReadMuscleClip(reader.Version, reader.Flags))
			{
				MuscleClipSize = reader.ReadUInt32();
				MuscleClip = new ClipMuscleConstant();
				MuscleClip.Read(reader);
			}
			if (IsReadClipBindingConstant(reader.Version))
			{
				ClipBindingConstant.Read(reader);
			}
#if UNIVERSAL
			if (IsReadAnimationClipSettings(reader.Version, reader.Flags))
			{
				AnimationClipSettings = new AnimationClipSettings();
				AnimationClipSettings.Read(reader);
			}
			if (IsReadEditorCurves(reader.Version, reader.Flags))
			{
				m_editorCurves = reader.ReadAssetArray<FloatCurve>();
				m_eulerEditorCurves = reader.ReadAssetArray<FloatCurve>();
			}
#endif

			if (IsReadHasGenericRootTransform(reader.Version, reader.Flags))
			{
				HasGenericRootTransform = reader.ReadBoolean();
			}
			if (IsReadHasMotionFloatCurves(reader.Version, reader.Flags))
			{
				HasMotionFloatCurves = reader.ReadBoolean();
			}
#if UNIVERSAL
			if (IsReadGenerateMotionCurves(reader.Version, reader.Flags))
			{
				GenerateMotionCurves = reader.ReadBoolean();
			}
#endif
			if (IsReadHasGenericRootTransform(reader.Version, reader.Flags))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadEvents(reader.Version))
			{
				m_events = reader.ReadAssetArray<AnimationEvent>();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

#if UNIVERSAL
			if (IsReadRuntimeEvents(reader.Version, reader.Flags))
			{
				m_runetimeEvents = reader.ReadAssetArray<AnimationEvent>();
			}
#endif
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			if (IsReadClassIDToTrack(file.Version, file.Flags))
			{
				foreach (KeyValuePair<int, PPtr<BaseAnimationTrack>> kvp in ClassIDToTrack)
				{
					yield return kvp.Value.FetchDependency(file, isLog, () => ValidName, ClassIDToTrackName);
				}
				foreach (ChildTrack track in ChildTracks)
				{
					foreach (Object asset in track.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
			if (IsReadCurves(file.Version))
			{
				foreach (FloatCurve curve in FloatCurves)
				{
					foreach (Object asset in curve.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
			if (IsReadPPtrCurves(file.Version))
			{
				foreach (PPtrCurve curve in PPtrCurves)
				{
					foreach (Object asset in curve.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
			if (IsReadClipBindingConstant(file.Version))
			{
				foreach (Object asset in ClipBindingConstant.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
#if UNIVERSAL
			if (IsReadAnimationClipSettings(file.Version, file.Flags))
			{
				foreach (Object asset in AnimationClipSettings.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
			if (IsReadEditorCurves(file.Version, file.Flags))
			{
				foreach (FloatCurve editorCurve in EditorCurves)
				{
					foreach (Object asset in editorCurve.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
				foreach (FloatCurve eulerEditorCurve in EulerEditorCurves)
				{
					foreach (Object asset in eulerEditorCurve.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
#endif
			if (IsReadEvents(file.Version))
			{
				foreach (AnimationEvent @event in Events)
				{
					foreach (Object asset in @event.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			foreach (Object asset in File.Collection.FetchAssets())
			{
				switch (asset.ClassID)
				{
					case ClassIDType.Avatar:
						{
							Avatar avatar = (Avatar)asset;
							if (ClipBindingConstant.IsAvatarMatch(avatar))
							{
								return avatar.TOS;
							}
						}
						break;

					case ClassIDType.Animator:
						Animator animator = (Animator)asset;
						if (IsAnimatorContainsClip(animator))
						{
							return animator.RetrieveTOS();
						}
						break;

					case ClassIDType.Animation:
						Animation animation = (Animation)asset;
						if (IsAnimationContainsClip(animation))
						{
							GameObject go = animation.GameObject.GetAsset(animation.File);
							return go.BuildTOS();
						}
						break;
				}
			}

			return new Dictionary<uint, string>() { { 0, string.Empty } };
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
			if(runetime == null)
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

		private bool IsExportGenericData(Version version, TransferInstructionFlags flags)
		{
			if (IsReadLegacy(version))
			{
				if (IsReadMuscleClip(version, flags))
				{
					return MuscleClip.Clip.IsValid(version);
				}
				return false;
			}
			if (IsReadAnimationType(version))
			{
				if (!IsReadClipBindingConstant(version))
				{
#warning TODO:
					return false;
				}
				if (IsReadMuscleClip(version, flags))
				{
					if (AnimationType != AnimationType.Legacy)
					{
						return MuscleClip.Clip.IsValid(version);
					}
				}
			}
			return false;
		}

		private bool GetLegacy(Version version)
		{
			if (IsReadLegacy(version))
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
			return IsReadCurves(version) ? RotationCurves : new QuaternionCurve[0];
		}
		private IReadOnlyList<CompressedAnimationCurve> GetCompressedRotationCurves(Version version)
		{
			return IsReadCompressedRotationCurves(version) ? CompressedRotationCurves : new CompressedAnimationCurve[0];
		}
		private IReadOnlyList<Vector3Curve> GetEulerCurves(Version version)
		{
			return IsReadEulerCurves(version) ? EulerCurves : new Vector3Curve[0];
		}
		private IReadOnlyList<Vector3Curve> GetPositionCurves(Version version)
		{
			return IsReadCurves(version) ? PositionCurves : new Vector3Curve[0];
		}
		private IReadOnlyList<Vector3Curve> GetScaleCurves(Version version)
		{
			return IsReadCurves(version) ? ScaleCurves : new Vector3Curve[0];
		}
		private IReadOnlyList<FloatCurve> GetFloatCurves(Version version)
		{
			return IsReadCurves(version) ? FloatCurves : new FloatCurve[0];
		}
		private IReadOnlyList<PPtrCurve> GetPPtrCurves(Version version)
		{
			return IsReadPPtrCurves(version) ? PPtrCurves : new PPtrCurve[0];
		}
		private AnimationClipBindingConstant GetClipBindingConstant(Version version)
		{
			return IsReadClipBindingConstant(version) ? ClipBindingConstant : new AnimationClipBindingConstant(true);
		}
		private AnimationClipSettings GetAnimationClipSettings(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadAnimationClipSettings(version, flags))
			{
				return AnimationClipSettings;
			}
#endif
			return IsReadMuscleClip(version, flags) ? new AnimationClipSettings(MuscleClip) : new AnimationClipSettings(true);
		}
		private IReadOnlyList<FloatCurve> GetEditorCurves(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorCurves(version, flags))
			{
				return EditorCurves;
			}
#endif
			return new FloatCurve[0];
		}
		private IReadOnlyList<FloatCurve> GetEulerEditorCurves(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorCurves(version, flags))
			{
				return EulerEditorCurves;
			}
#endif
			return new FloatCurve[0];
		}
		private bool GetGenerateMotionCurves(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadGenerateMotionCurves(version, flags))
			{
				return GenerateMotionCurves;
			}
#endif
			return false;
		}
		private IReadOnlyList<AnimationEvent> GetEvents(Version version)
		{
			return IsReadEvents(version) ? m_events : new AnimationEvent[0];
		}

		public override string ExportExtension => "anim";

		public IReadOnlyDictionary<int, PPtr<BaseAnimationTrack>> ClassIDToTrack => m_classIDToTrack;
		public IReadOnlyList<ChildTrack> ChildTracks => m_childTracks;
		public AnimationType AnimationType { get; private set; }
		public bool Legacy { get; private set; }
		public bool Compressed { get; private set; }
		public bool UseHightQualityCurve { get; private set; }
		public IReadOnlyList<QuaternionCurve> RotationCurves => m_rotationCurves;
		public IReadOnlyList<CompressedAnimationCurve> CompressedRotationCurves => m_compressedRotationCurves;
		public IReadOnlyList<Vector3Curve> EulerCurves => m_eulerCurves;
		public IReadOnlyList<Vector3Curve> PositionCurves => m_positionCurves;
		public IReadOnlyList<Vector3Curve> ScaleCurves => m_scaleCurves;
		public IReadOnlyList<FloatCurve> FloatCurves => m_floatCurves;
		public IReadOnlyList<PPtrCurve> PPtrCurves => m_PPtrCurves;
		public float SampleRate { get; private set; }
		public WrapMode WrapMode { get; private set; }
		public uint MuscleClipSize { get; private set; }
		public ClipMuscleConstant MuscleClip { get; private set; }
#if UNIVERSAL
		public AnimationClipSettings AnimationClipSettings { get; private set; }
		private IReadOnlyList<FloatCurve> EditorCurves => m_editorCurves;
		private IReadOnlyList<FloatCurve> EulerEditorCurves => m_eulerEditorCurves;
#endif
		public bool HasGenericRootTransform { get; private set; }
		public bool HasMotionFloatCurves { get; private set; }
#if UNIVERSAL
		public bool GenerateMotionCurves { get; private set; }
#endif
		public IReadOnlyList<AnimationEvent> Events => m_events;

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
		public const string EventsName = "m_Events";

		public AABB Bounds;
		public AnimationClipBindingConstant ClipBindingConstant;

		private Dictionary<int, PPtr<BaseAnimationTrack>> m_classIDToTrack;
		private ChildTrack[] m_childTracks;
		private QuaternionCurve[] m_rotationCurves;
		private CompressedAnimationCurve[] m_compressedRotationCurves;
		private Vector3Curve[] m_eulerCurves;
		private Vector3Curve[] m_positionCurves;
		private Vector3Curve[] m_scaleCurves;
		private FloatCurve[] m_floatCurves;
		private PPtrCurve[] m_PPtrCurves;
#if UNIVERSAL
		private FloatCurve[] m_editorCurves;
		private FloatCurve[] m_eulerEditorCurves;
#endif
		private AnimationEvent[] m_events;
#if UNIVERSAL
		private AnimationEvent[] m_runetimeEvents;
#endif
	}
}
