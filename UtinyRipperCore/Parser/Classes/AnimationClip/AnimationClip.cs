using System.Collections.Generic;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimationClips;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
		public static bool IsReadClassIDToTrack(Version version)
		{
			return version.IsLess(2);
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
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadMuscleClipSize(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadClipBindingConstant(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadEvents(Version version)
		{
			return version.IsGreaterEqual(2, 1);
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 6;
			}

#warning unknown
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Beta))
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

			if (IsReadClassIDToTrack(reader.Version))
			{
				m_classIDToTrack = new Dictionary<int, PPtr<BaseAnimationTrack>>();
				m_classIDToTrack.Read(reader);
				m_childTracks = reader.ReadArray<ChildTrack>();
			}

			if (IsReadAnimationType(reader.Version))
			{
				AnimationType = (AnimationType)reader.ReadInt32();
			}
			if(IsReadLegacy(reader.Version))
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
				m_rotationCurves = reader.ReadArray<QuaternionCurve>();
			}
			if (IsReadCompressedRotationCurves(reader.Version))
			{
				m_compressedRotationCurves = reader.ReadArray<CompressedAnimationCurve>();
			}
			if (IsReadEulerCurves(reader.Version))
			{
				m_eulerCurves = reader.ReadArray<Vector3Curve>();
			}
			if (IsReadCurves(reader.Version))
			{
				m_positionCurves = reader.ReadArray<Vector3Curve>();
				m_scaleCurves = reader.ReadArray<Vector3Curve>();
				m_floatCurves = reader.ReadArray<FloatCurve>();
			}
			if (IsReadPPtrCurves(reader.Version))
			{
				m_PPtrCurves = reader.ReadArray<PPtrCurve>();
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
			if (IsReadMuscleClipSize(reader.Version))
			{
				MuscleClipSize = reader.ReadUInt32();
				MuscleClip.Read(reader);
			}
			if (IsReadClipBindingConstant(reader.Version))
			{
				ClipBindingConstant.Read(reader);
			}

			if (IsReadEvents(reader.Version))
			{
				m_events = reader.ReadArray<AnimationEvent>();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			if (IsReadCurves(file.Version))
			{
				foreach (FloatCurve curve in FloatCurves)
				{
					foreach (Object @object in curve.FetchDependencies(file, isLog))
					{
						yield return @object;
					}
				}
			}
			if (IsReadPPtrCurves(file.Version))
			{
				foreach (PPtrCurve curve in PPtrCurves)
				{
					foreach(Object @object in curve.FetchDependencies(file, isLog))
					{
						yield return @object;
					}
				}
			}
			if (IsReadClipBindingConstant(file.Version))
			{
				foreach (Object @object in ClipBindingConstant.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
			if (IsReadEvents(file.Version))
			{
				foreach (AnimationEvent @event in Events)
				{
					foreach (Object @object in @event.FetchDependencies(file, isLog))
					{
						yield return @object;
					}
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Legacy", GetLegacy(container.Version));
			node.Add("m_Compressed", Compressed);
			node.Add("m_UseHighQualityCurve", UseHightQualityCurve);

			AnimationCurves curves = GetAnimationCurves(container.Version, container.Platform);
			node.Add("m_RotationCurves", curves.RotationCurves.ExportYAML(container));
			node.Add("m_CompressedRotationCurves", curves.RotationCurves.ExportYAML(container));
			node.Add("m_EulerCurves", curves.EulerCurves.ExportYAML(container));
			node.Add("m_PositionCurves", curves.PositionCurves.ExportYAML(container));
			node.Add("m_ScaleCurves", curves.ScaleCurves.ExportYAML(container));
			node.Add("m_FloatCurves", curves.FloatCurves.ExportYAML(container));
			node.Add("m_PPtrCurves", curves.PPtrCurves.ExportYAML(container));

			node.Add("m_SampleRate", SampleRate);
			node.Add("m_WrapMode", (int)WrapMode);
			node.Add("m_Bounds", Bounds.ExportYAML(container));
			node.Add("m_ClipBindingConstant", ClipBindingConstant.ExportYAML(container));
			node.Add("m_AnimationClipSettings", MuscleClip.ExportYAML(container));
			node.Add("m_EditorCurves", YAMLSequenceNode.Empty);
			node.Add("m_EulerEditorCurves", YAMLSequenceNode.Empty);
			node.Add("m_HasGenericRootTransform", false);
			node.Add("m_HasMotionFloatCurves", false);
			node.Add("m_GenerateMotionCurves", false);
			node.Add("m_Events", IsReadEvents(container.Version) ? m_events.ExportYAML(container) : YAMLSequenceNode.Empty);
			
			return node;
		}

		private AnimationCurves ExportGenericData()
		{
			IReadOnlyDictionary<uint, string> tos = FindTOS();

			AnimationClipGenericConverter converter = new AnimationClipGenericConverter(File.Version, File.Platform, File.Flags);
			converter.Process(MuscleClip.Clip, ClipBindingConstant, tos);

			return new AnimationCurves()
			{
				RotationCurves = converter.Rotations.Union(GetRotationCurves(File.Version)),
				CompressedRotationCurves = GetCompressedRotationCurves(File.Version),
				EulerCurves = converter.Eulers.Union(GetEulerCurves(File.Version)),
				PositionCurves = converter.Translations.Union(GetPositionCurves(File.Version)),
				ScaleCurves = converter.Scales.Union(GetScaleCurves(File.Version)),
				FloatCurves = converter.Floats.Union(GetFloatCurves(File.Version)),
				PPtrCurves = GetPPtrCurves(File.Version),
			};
		}
		
		private IReadOnlyDictionary<uint, string> FindTOS()
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

#warning what about humanoid?
		private bool IsExportGenericData(Version version)
		{
			if (IsReadLegacy(version))
			{
				return MuscleClip.Clip.IsValid(version);
			}
			if (IsReadAnimationType(version))
			{
				if(AnimationType != AnimationType.Legacy)
				{
					return MuscleClip.Clip.IsValid(version);
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

		private AnimationCurves GetAnimationCurves(Version version, Platform platform)
		{
			if (IsExportGenericData(version))
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
		public IReadOnlyList<AnimationEvent> Events => m_events;
				
		public AABB Bounds;
		public ClipMuscleConstant MuscleClip;
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
		private AnimationEvent[] m_events;
	}
}
