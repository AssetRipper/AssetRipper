using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimationClips;
using UtinyRipper.Classes.AnimationClips.Editor;
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
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadEulerCurves(Version version)
		{
			return version.IsGreaterEqual(5);
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadClassIDToTrack(stream.Version))
			{
				m_classIDToTrack = new Dictionary<int, PPtr<BaseAnimationTrack>>();
				m_classIDToTrack.Read(stream);
				m_childTracks = stream.ReadArray<ChildTrack>();
			}

			if (IsReadAnimationType(stream.Version))
			{
				AnimationType = (AnimationType)stream.ReadInt32();
			}
			if(IsReadLegacy(stream.Version))
			{
				Legacy = stream.ReadBoolean();
			}

			if (IsReadCompressed(stream.Version))
			{
				Compressed = stream.ReadBoolean();
			}
			if (IsReadUseHightQualityCurve(stream.Version))
			{
				UseHightQualityCurve = stream.ReadBoolean();
			}
			if (IsAlignCompressed(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadCurves(stream.Version))
			{
				m_rotationCurves = stream.ReadArray<QuaternionCurve>();
			}
			if (IsReadCompressedRotationCurves(stream.Version))
			{
				m_compressedRotationCurves = stream.ReadArray<CompressedAnimationCurve>();
			}
			if (IsReadEulerCurves(stream.Version))
			{
				m_eulerCurves = stream.ReadArray<Vector3Curve>();
			}
			if (IsReadCurves(stream.Version))
			{
				m_positionCurves = stream.ReadArray<Vector3Curve>();
				m_scaleCurves = stream.ReadArray<Vector3Curve>();
				m_floatCurves = stream.ReadArray<FloatCurve>();
			}
			if (IsReadPPtrCurves(stream.Version))
			{
				m_PPtrCurves = stream.ReadArray<PPtrCurve>();
			}

			if (IsReadSampleRate(stream.Version))
			{
				SampleRate = stream.ReadSingle();
			}

			if (IsReadWrapMode(stream.Version))
			{
				WrapMode = (WrapMode)stream.ReadInt32();
			}
			if (IsReadBounds(stream.Version))
			{
				Bounds.Read(stream);
			}
			if (IsReadMuscleClipSize(stream.Version))
			{
				MuscleClipSize = stream.ReadUInt32();
				MuscleClip.Read(stream);
			}
			if (IsReadClipBindingConstant(stream.Version))
			{
				ClipBindingConstant.Read(stream);
			}

			if (IsReadEvents(stream.Version))
			{
				m_events = stream.ReadArray<AnimationEvent>();
			}
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
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

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Legacy", IsReadLegacy(exporter.Version) ? Legacy : true);
			node.Add("m_Compressed", Compressed);
			node.Add("m_UseHighQualityCurve", UseHightQualityCurve);

			if(IsExportGenericData(exporter.Version))
			{
				ExportGenericData(exporter, node);
			}
			else
			{
				node.Add("m_RotationCurves", IsReadCurves(exporter.Version) ? m_rotationCurves.ExportYAML(exporter) : YAMLSequenceNode.Empty);
				node.Add("m_CompressedRotationCurves", IsReadCompressedRotationCurves(exporter.Version) ? m_compressedRotationCurves.ExportYAML(exporter) : YAMLSequenceNode.Empty);
				node.Add("m_EulerCurves", IsReadEulerCurves(exporter.Version) ? m_eulerCurves.ExportYAML(exporter) : YAMLSequenceNode.Empty);
				node.Add("m_PositionCurves", IsReadCurves(exporter.Version) ? m_positionCurves.ExportYAML(exporter) : YAMLSequenceNode.Empty);
				node.Add("m_ScaleCurves",  IsReadCurves(exporter.Version) ?  m_scaleCurves.ExportYAML(exporter) : YAMLSequenceNode.Empty);
				node.Add("m_FloatCurves", IsReadCurves(exporter.Version) ? m_floatCurves.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			}
			
			node.Add("m_PPtrCurves", IsReadPPtrCurves(exporter.Version) ? m_PPtrCurves.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			node.Add("m_SampleRate", SampleRate);
			node.Add("m_WrapMode", (int)WrapMode);
			node.Add("m_Bounds", Bounds.ExportYAML(exporter));
			node.Add("m_ClipBindingConstant", ClipBindingConstant.ExportYAML(exporter));
			node.Add("m_AnimationClipSettings", MuscleClip.ExportYAML(exporter));
			node.Add("m_EditorCurves", YAMLSequenceNode.Empty);
			node.Add("m_EulerEditorCurves", YAMLSequenceNode.Empty);
			node.Add("m_HasGenericRootTransform", false);
			node.Add("m_HasMotionFloatCurves", false);
			node.Add("m_GenerateMotionCurves", false);
			node.Add("m_Events", IsReadEvents(exporter.Version) ? m_events.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			
			return node;
		}

		private void ExportGenericData(IAssetsExporter exporter, YAMLMappingNode node)
		{
			IReadOnlyDictionary<uint, string> tos = FindTOS();
			/*if(tos == null)
			{
				ExportEmptyGenericData(node);
				return;
			}*/

			ExportGenericData(exporter, node, tos);
		}

#warning TODO: it's too complicated and unintuitive. need to simplify
		private void ExportGenericData(IAssetsExporter exporter, YAMLMappingNode node, IReadOnlyDictionary<uint, string> tos)
		{
			StreamedClip streamedClip = MuscleClip.Clip.StreamedClip;
			DenseClip denseClip = MuscleClip.Clip.DenseClip;
			ConstantClip constantClip = MuscleClip.Clip.ConstantClip;

			IReadOnlyList<StreamedFrame> streamedFrames = streamedClip.GenerateFrames(exporter);
			Dictionary<uint, Vector3Curve> translations = new Dictionary<uint, Vector3Curve>();
			Dictionary<uint, QuaternionCurve> rotations = new Dictionary<uint, QuaternionCurve>();
			Dictionary<uint, Vector3Curve> scales = new Dictionary<uint, Vector3Curve>();
			Dictionary<uint, Vector3Curve> eulers = new Dictionary<uint, Vector3Curve>();
			Dictionary<uint, FloatCurve> floats = new Dictionary<uint, FloatCurve>();

			int frameCount = Math.Max(denseClip.FrameCount - 1, streamedFrames.Count - 2);
			float[] frameCurvesValue = new float[streamedClip.CurveCount];
			for (int frame = 0, streamFrame = 1; frame < frameCount; frame++, streamFrame++)
			{
				bool isAdd = true;
				float time;
				StreamedFrame streamedFrame = new StreamedFrame();
				if (streamFrame < streamedFrames.Count)
				{
					streamedFrame = streamedFrames[streamFrame];
					time = streamedFrame.Time;
				}
				else
				{
					time = (float)frame / SampleRate;
				}

				bool isStreamFrame = streamFrame < (streamedFrames.Count - 1);
				bool isDenseFrame = frame < (denseClip.FrameCount - 1);

				// number of stream curves which has key in current frame
				int streamFrameCurveCount = isStreamFrame ? streamedFrame.Curves.Count : 0;
				int denseFrameCurveCount = (int)denseClip.CurveCount;
				// total amount of curves which has key in current frame
				int frameCurveCount = streamFrameCurveCount + denseFrameCurveCount + constantClip.Constants.Count;
				int streamOffset = (int)streamedClip.CurveCount - streamFrameCurveCount;
				for (int curve = 0; curve < frameCurveCount;)
				{
					int curveIndex;
					IReadOnlyList<float> curvesValue;
					int offset;

					if (isStreamFrame && curve < streamedFrame.Curves.Count)
					{
#warning TODO: read TCB and convert to in/out slope
						for (int key = curve; key < Math.Min(curve + 5, streamedFrame.Curves.Count); key++)
						{
							frameCurvesValue[key] = streamedFrame.Curves[key].Value;
						}
						curveIndex = streamedFrame.Curves[curve].Index;
						curvesValue = frameCurvesValue;
						offset = 0;
					}
					else if (isDenseFrame && curve < streamFrameCurveCount + denseFrameCurveCount)
					{
						curveIndex = curve + streamOffset;
						curvesValue = denseClip.SampleArray;
						offset = streamFrameCurveCount - frame * denseFrameCurveCount;
					}
					else if (!isDenseFrame && curve < streamFrameCurveCount + denseFrameCurveCount)
					{
						curve += denseFrameCurveCount;

						curveIndex = curve + streamOffset;
						curvesValue = constantClip.Constants;
						offset = streamFrameCurveCount + denseFrameCurveCount;
						isAdd = frame == 0 || frame == frameCount - 1;
					}
					else
					{
						curveIndex = curve + streamOffset;
						curvesValue = constantClip.Constants;
						offset = streamFrameCurveCount + denseFrameCurveCount;
						isAdd = frame == 0 || frame == frameCount - 1;
					}

					GenericBinding binding = ClipBindingConstant.FindBinding(curveIndex);
					uint pathHash = binding.Path;

					if (pathHash == 0)
					{
						curve++;
						continue;
					}
					if (!tos.TryGetValue(pathHash, out string path))
					{
						path = "dummy" + pathHash;
						//Logger.Log(LogType.Debug, LogCategory.Export, $"Can't find path '{binding.Path}' in TOS for {ToLogString()}");
					}

					switch (binding.BindingType)
					{
						case BindingType.Translation:
							float x = curvesValue[curve++ - offset];
							float y = curvesValue[curve++ - offset];
							float z = curvesValue[curve++ - offset];
							float w = 0;
							if (isAdd)
							{
								Vector3f trans = new Vector3f(x, y, z);
								if (!translations.TryGetValue(pathHash, out Vector3Curve transCurve))
								{
									transCurve = new Vector3Curve(path);
									translations[pathHash] = transCurve;
								}

								KeyframeTpl<Vector3f> transKey = new KeyframeTpl<Vector3f>(time, trans);
								transCurve.Curve.Curve.Add(transKey);
							}
							break;

						case BindingType.Rotation:
							x = curvesValue[curve++ - offset];
							y = curvesValue[curve++ - offset];
							z = curvesValue[curve++ - offset];
							w = curvesValue[curve++ - offset];
							if (isAdd)
							{
								Quaternionf rot = new Quaternionf(x, y, z, w);
								if (!rotations.TryGetValue(pathHash, out QuaternionCurve rotCurve))
								{
									rotCurve = new QuaternionCurve(path);
									rotations[pathHash] = rotCurve;
								}

								KeyframeTpl<Quaternionf> rotKey = new KeyframeTpl<Quaternionf>(time, rot);
								rotCurve.Curve.Curve.Add(rotKey);
							}
							break;

						case BindingType.Scaling:
							x = curvesValue[curve++ - offset];
							y = curvesValue[curve++ - offset];
							z = curvesValue[curve++ - offset];
							if(isAdd)
							{
								Vector3f scale = new Vector3f(x, y, z);
								if (!scales.TryGetValue(pathHash, out Vector3Curve scaleCurve))
								{
									scaleCurve = new Vector3Curve(path);
									scales[pathHash] = scaleCurve;
								}

								KeyframeTpl<Vector3f> scaleKey = new KeyframeTpl<Vector3f>(time, scale);
								scaleCurve.Curve.Curve.Add(scaleKey);
							}
							break;

						case BindingType.EulerRotation:
							x = curvesValue[curve++ - offset];
							y = curvesValue[curve++ - offset];
							z = curvesValue[curve++ - offset];
							if (isAdd)
							{
								Vector3f euler = new Vector3f(x, y, z);
								if (!eulers.TryGetValue(pathHash, out Vector3Curve eulerCurve))
								{
									eulerCurve = new Vector3Curve(path);
									eulers[pathHash] = eulerCurve;
								}

								KeyframeTpl<Vector3f> eulerKey = new KeyframeTpl<Vector3f>(time, euler);
								eulerCurve.Curve.Curve.Add(eulerKey);
							}
							break;

						case BindingType.Floats:
							float value = curvesValue[curve++ - offset];
							if (isAdd)
							{
								Float @float = new Float(value);
								if (!floats.TryGetValue(pathHash, out FloatCurve floatCurve))
								{
									floatCurve = new FloatCurve(path);
									floats[pathHash] = floatCurve;
								}

								KeyframeTpl<Float> floatKey = new KeyframeTpl<Float>(time, @float);
								floatCurve.Curve.Curve.Add(floatKey);
							}
							break;

						default:
#warning TODO: ???
							curve++;
							//throw new NotImplementedException(binding.BindingType.ToString());
							break;
					}
				}
			}

			node.Add("m_RotationCurves", rotations.Values.ExportYAML(exporter));
			node.Add("m_CompressedRotationCurves", YAMLSequenceNode.Empty);
			node.Add("m_EulerCurves", eulers.Values.ExportYAML(exporter));
			node.Add("m_PositionCurves", translations.Values.ExportYAML(exporter));
			node.Add("m_ScaleCurves", scales.Values.ExportYAML(exporter));
			node.Add("m_FloatCurves", floats.Values.ExportYAML(exporter));
		}

		/*private void ExportEmptyGenericData(YAMLMappingNode node)
		{
			node.Add("m_RotationCurves", YAMLMappingNode.Empty);
			node.Add("m_CompressedRotationCurves", YAMLMappingNode.Empty);
			node.Add("m_EulerCurves", YAMLMappingNode.Empty);
			node.Add("m_PositionCurves", YAMLMappingNode.Empty);
			node.Add("m_ScaleCurves", YAMLMappingNode.Empty);
			node.Add("m_FloatCurves", YAMLMappingNode.Empty);
		}*/

		private IReadOnlyDictionary<uint, string> FindTOS()
		{
			Avatar avatar = FindAvatar();
			if(avatar == null)
			{
				//Logger.Log(LogType.Warning, LogCategory.Export, $"Avatar for {ToLogString()} wasn't found");
			}
			else
			{
				return avatar.TOS;
			}

#warning TODO: build TOS with transforms
			return new Dictionary<uint, string>() { { 0, string.Empty } };
		}

		private Avatar FindAvatar()
		{
			foreach (Object @object in File.Collection.FetchAssets())
			{
				if (@object.ClassID != ClassIDType.Animator)
				{
					continue;
				}

				Animator animator = (Animator)@object;
				RuntimeAnimatorController runetime = animator.Controller.FindObject(animator.File);
				switch (runetime)
				{
					case null:
						continue;

					case AnimatorOverrideController @override:
						foreach (var clip in @override.Clips)
						{
							if (clip.OverrideClip.IsObject(@override.File, this))
							{
								return animator.Avatar.FindObject(animator.File);
							}
						}
						break;

					default:
						AnimatorController controller = (AnimatorController)runetime;
						foreach (PPtr<AnimationClip> clip in controller.AnimationClips)
						{
							if (clip.IsObject(controller.File, this))
							{
								return animator.Avatar.FindObject(animator.File);
							}
						}
						break;
				}
			}
			return null;
		}

#warning what about humanoid?
		private bool IsExportGenericData(Version version)
		{
			if (IsReadAnimationType(version) && AnimationType == AnimationType.Mecanim)
			{
				return true;
			}
			if (IsReadLegacy(version))
			{
				if (MuscleClip.Clip.IsValid(version))
				{
					return true;
				}
			}
			return false;
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
