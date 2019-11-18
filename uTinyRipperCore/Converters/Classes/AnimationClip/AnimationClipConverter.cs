using SevenZip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using uTinyRipper.Classes;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters.AnimationClips;
using uTinyRipper.Layout;
using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Converters
{
	public class AnimationClipConverter
	{
		private AnimationClipConverter(AnimationClip clip)
		{
			if (clip == null)
			{
				throw new ArgumentNullException(nameof(clip));
			}
			m_clip = clip;
			m_customCurveResolver = new CustomCurveResolver(clip);
		}

		public static AnimationClipConverter Process(AnimationClip clip)
		{
			AnimationClipConverter converter = new AnimationClipConverter(clip);
			converter.ProcessInner();
			return converter;
		}

		private void ProcessInner()
		{
			Clip clip = m_clip.MuscleClip.Clip;
			AnimationClipBindingConstant bindings = m_clip.ClipBindingConstant;
			IReadOnlyDictionary<uint, string> tos = m_clip.FindTOS();

			IReadOnlyList<StreamedFrame> streamedFrames = clip.StreamedClip.GenerateFrames(Layout);
			float lastDenseFrame = clip.DenseClip.FrameCount / clip.DenseClip.SampleRate;
			float lastSampleFrame = streamedFrames.Count > 1 ? streamedFrames[streamedFrames.Count - 2].Time : 0.0f;
			float lastFrame = Math.Max(lastDenseFrame, lastSampleFrame);

			ProcessStreams(streamedFrames, bindings, tos, clip.DenseClip.SampleRate);
			ProcessDenses(clip, bindings, tos);
			if (Clip.HasConstantClip(Layout.Info.Version))
			{
				ProcessConstant(clip, bindings, tos, lastFrame);
			}
			CreateCurves();
		}

		private void CreateCurves()
		{
			Translations = m_translations.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
			Rotations = m_rotations.Select(t => new QuaternionCurve(t.Key, t.Value)).ToArray();
			Scales = m_scales.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
			Eulers = m_eulers.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
			Floats = m_floats.Select(t => new FloatCurve(t.Key, t.Value)).ToArray();
			PPtrs = m_pptrs.Select(t => new PPtrCurve(t.Key, t.Value)).ToArray();
		}

		private void ProcessStreams(IReadOnlyList<StreamedFrame> streamFrames, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos, float sampleRate)
		{
			float[] curveValues = new float[4];
			float[] inSlopeValues = new float[4];
			float[] outSlopeValues = new float[4];
			float interval = 1.0f / sampleRate;

			// first (index [0]) stream frame is for slope calculation for the first real frame (index [1])
			// last one (index [count - 1]) is +Infinity
			// it is made for slope processing, but we don't need them
			for (int frameIndex = 1; frameIndex < streamFrames.Count - 1; frameIndex++)
			{
				StreamedFrame frame = streamFrames[frameIndex];
				for (int curveIndex = 0; curveIndex < frame.Curves.Length;)
				{
					StreamedCurveKey curve = frame.Curves[curveIndex];
					GenericBinding binding = bindings.FindBinding(curve.Index);

					string path = GetCurvePath(tos, binding.Path);
					if (binding.IsTransform)
					{
						GetPreviousFrame(streamFrames, curve.Index, frameIndex, out int prevFrameIndex, out int prevCurveIndex);
						int dimension = binding.TransformType.GetDimension();
						for (int key = 0; key < dimension; key++)
						{
							StreamedCurveKey keyCurve = frame.Curves[curveIndex];
							StreamedFrame prevFrame = streamFrames[prevFrameIndex];
							StreamedCurveKey prevKeyCurve = prevFrame.Curves[prevCurveIndex + key];
							float deltaTime = frame.Time - prevFrame.Time;
							curveValues[key] = keyCurve.Value;
							inSlopeValues[key] = prevKeyCurve.CalculateNextInSlope(deltaTime, keyCurve.Value);
							outSlopeValues[key] = keyCurve.OutSlope;
							curveIndex = GetNextCurve(frame, curveIndex);
						}

						AddTransformCurve(frame.Time, binding.TransformType, curveValues, inSlopeValues, outSlopeValues, 0, path);
					}
					else if (binding.CustomType == BindingCustomType.None)
					{
						AddDefaultCurve(binding, path, frame.Time, frame.Curves[curveIndex].Value);
						curveIndex = GetNextCurve(frame, curveIndex);
					}
					else
					{
						AddCustomCurve(bindings, binding, path, frame.Time, frame.Curves[curveIndex].Value);
						curveIndex = GetNextCurve(frame, curveIndex);
					}
				}
			}
		}

		private void ProcessDenses(Clip clip, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos)
		{
			DenseClip dense = clip.DenseClip;
			int streamCount = clip.StreamedClip.CurveCount;
			float[] slopeValues = new float[4]; // no slopes - 0 values
			for (int frameIndex = 0; frameIndex < dense.FrameCount; frameIndex++)
			{
				float time = frameIndex / dense.SampleRate;
				int frameOffset = frameIndex * dense.CurveCount;
				for (int curveIndex = 0; curveIndex < dense.CurveCount;)
				{
					int index = streamCount + curveIndex;
					GenericBinding binding = bindings.FindBinding(index);
					string path = GetCurvePath(tos, binding.Path);
					int framePosition = frameOffset + curveIndex;
					if (binding.IsTransform)
					{
						AddTransformCurve(time, binding.TransformType, dense.SampleArray, slopeValues, slopeValues, framePosition, path);
						curveIndex += binding.TransformType.GetDimension();
					}
					else if (binding.CustomType == BindingCustomType.None)
					{
						AddDefaultCurve(binding, path, time, dense.SampleArray[framePosition]);
						curveIndex++;
					}
					else
					{
						AddCustomCurve(bindings, binding, path, time, dense.SampleArray[framePosition]);
						curveIndex++;
					}
				}
			}
		}

		private void ProcessConstant(Clip clip, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos, float lastFrame)
		{
			ConstantClip constant = clip.ConstantClip;
			int streamCount = clip.StreamedClip.CurveCount;
			int denseCount = clip.DenseClip.CurveCount;
			float[] slopeValues = new float[4]; // no slopes - 0 values

			// only first and last frames
			float time = 0.0f;
			for (int i = 0; i < 2; i++, time += lastFrame)
			{
				for (int curveIndex = 0; curveIndex < constant.Constants.Length;)
				{
					int index = streamCount + denseCount + curveIndex;
					GenericBinding binding = bindings.FindBinding(index);
					string path = GetCurvePath(tos, binding.Path);
					if (binding.IsTransform)
					{
						AddTransformCurve(time, binding.TransformType, constant.Constants, slopeValues, slopeValues, curveIndex, path);
						curveIndex += binding.TransformType.GetDimension();
					}
					else if (binding.CustomType == BindingCustomType.None)
					{
						AddDefaultCurve(binding, path, time, constant.Constants[curveIndex]);
						curveIndex++;
					}
					else
					{
						AddCustomCurve(bindings, binding, path, time, constant.Constants[curveIndex]);
						curveIndex++;
					}
				}
			}
		}

		private void AddCustomCurve(AnimationClipBindingConstant bindings, GenericBinding binding, string path, float time, float value)
		{
			switch (binding.CustomType)
			{
				case BindingCustomType.AnimatorMuscle:
					AddAnimatorMuscleCurve(binding, time, value);
					break;

				default:
					string attribute = m_customCurveResolver.ToAttributeName(Layout, binding.CustomType, binding.Attribute, path);
					if (binding.IsPPtrCurve)
					{
						PPtrCurve curve = new PPtrCurve(path, attribute, binding.ClassID, binding.Script.CastTo<MonoScript>());
						AddPPtrKeyframe(curve, bindings, time, (int)value);
					}
					else
					{
						FloatCurve curve = new FloatCurve(path, attribute, binding.ClassID, binding.Script.CastTo<MonoScript>());
						AddFloatKeyframe(curve, time, value);
					}
					break;
			}
		}

		private void AddTransformCurve(float time, TransformType transType, IReadOnlyList<float> curveValues,
			IReadOnlyList<float> inSlopeValues, IReadOnlyList<float> outSlopeValues, int offset, string path)
		{
			switch (transType)
			{
				case TransformType.Translation:
					{
						Vector3Curve curve = new Vector3Curve(path);
						if (!m_translations.TryGetValue(curve, out List<KeyframeTpl<Vector3f>> transCurve))
						{
							transCurve = new List<KeyframeTpl<Vector3f>>();
							m_translations.Add(curve, transCurve);
						}

						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];

						float inX = inSlopeValues[0];
						float inY = inSlopeValues[1];
						float inZ = inSlopeValues[2];

						float outX = outSlopeValues[0];
						float outY = outSlopeValues[1];
						float outZ = outSlopeValues[2];

						Vector3f value = new Vector3f(x, y, z);
						Vector3f inSlope = new Vector3f(inX, inY, inZ);
						Vector3f outSlope = new Vector3f(outX, outY, outZ);
						KeyframeTpl<Vector3f> transKey = new KeyframeTpl<Vector3f>(time, value, inSlope, outSlope, KeyframeTpl<Vector3f>.DefaultVector3Weight);
						transCurve.Add(transKey);
					}
					break;

				case TransformType.Rotation:
					{
						QuaternionCurve curve = new QuaternionCurve(path);
						if (!m_rotations.TryGetValue(curve, out List<KeyframeTpl<Quaternionf>> rotCurve))
						{
							rotCurve = new List<KeyframeTpl<Quaternionf>>();
							m_rotations.Add(curve, rotCurve);
						}

						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];
						float w = curveValues[offset + 3];

						float inX = inSlopeValues[0];
						float inY = inSlopeValues[1];
						float inZ = inSlopeValues[2];
						float inW = inSlopeValues[3];

						float outX = outSlopeValues[0];
						float outY = outSlopeValues[1];
						float outZ = outSlopeValues[2];
						float outW = outSlopeValues[3];

						Quaternionf value = new Quaternionf(x, y, z, w);
						Quaternionf inSlope = new Quaternionf(inX, inY, inZ, inW);
						Quaternionf outSlope = new Quaternionf(outX, outY, outZ, outW);
						KeyframeTpl<Quaternionf> rotKey = new KeyframeTpl<Quaternionf>(time, value, inSlope, outSlope, KeyframeTpl<Quaternionf>.DefaultQuaternionWeight);
						rotCurve.Add(rotKey);
					}
					break;

				case TransformType.Scaling:
					{
						Vector3Curve curve = new Vector3Curve(path);
						if (!m_scales.TryGetValue(curve, out List<KeyframeTpl<Vector3f>> scaleCurve))
						{
							scaleCurve = new List<KeyframeTpl<Vector3f>>();
							m_scales.Add(curve, scaleCurve);
						}

						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];

						float inX = inSlopeValues[0];
						float inY = inSlopeValues[1];
						float inZ = inSlopeValues[2];

						float outX = outSlopeValues[0];
						float outY = outSlopeValues[1];
						float outZ = outSlopeValues[2];

						Vector3f value = new Vector3f(x, y, z);
						Vector3f inSlope = new Vector3f(inX, inY, inZ);
						Vector3f outSlope = new Vector3f(outX, outY, outZ);
						KeyframeTpl<Vector3f> scaleKey = new KeyframeTpl<Vector3f>(time, value, inSlope, outSlope, KeyframeTpl<Vector3f>.DefaultVector3Weight);
						scaleCurve.Add(scaleKey);
					}
					break;

				case TransformType.EulerRotation:
					{
						Vector3Curve curve = new Vector3Curve(path);
						if (!m_eulers.TryGetValue(curve, out List<KeyframeTpl<Vector3f>> eulerCurve))
						{
							eulerCurve = new List<KeyframeTpl<Vector3f>>();
							m_eulers.Add(curve, eulerCurve);
						}

						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];

						float inX = inSlopeValues[0];
						float inY = inSlopeValues[1];
						float inZ = inSlopeValues[2];

						float outX = outSlopeValues[0];
						float outY = outSlopeValues[1];
						float outZ = outSlopeValues[2];

						Vector3f value = new Vector3f(x, y, z);
						Vector3f inSlope = new Vector3f(inX, inY, inZ);
						Vector3f outSlope = new Vector3f(outX, outY, outZ);
						KeyframeTpl<Vector3f> eulerKey = new KeyframeTpl<Vector3f>(time, value, inSlope, outSlope, KeyframeTpl<Vector3f>.DefaultVector3Weight);
						eulerCurve.Add(eulerKey);
					}
					break;

				default:
					throw new NotImplementedException(transType.ToString());
			}
		}

		private void AddDefaultCurve(GenericBinding binding, string path, float time, float value)
		{
			switch (binding.ClassID)
			{
				case ClassIDType.GameObject:
					{
						AddGameObjectCurve(binding, path, time, value);
					}
					break;

				case ClassIDType.MonoBehaviour:
					{
						AddScriptCurve(binding, path, time, value);
					}
					break;

				default:
					AddEngineCurve(binding, path, time, value);
					break;
			}
		}

		private void AddGameObjectCurve(GenericBinding binding, string path, float time, float value)
		{
			if (binding.Attribute == CRC.CalculateDigestAscii(Layout.GameObject.IsActiveName))
			{
				FloatCurve curve = new FloatCurve(path, Layout.GameObject.IsActiveName, ClassIDType.GameObject, default);
				AddFloatKeyframe(curve, time, value);
				return;
			}
			else
			{
				// that means that dev exported animation clip with missing component
				FloatCurve curve = new FloatCurve(path, MissedPropertyPrefix + binding.Attribute, ClassIDType.GameObject, default);
				AddFloatKeyframe(curve, time, value);
			}
		}

		private void AddScriptCurve(GenericBinding binding, string path, float time, float value)
		{
#warning TODO:
			FloatCurve curve = new FloatCurve(path, ScriptPropertyPrefix + binding.Attribute, ClassIDType.MonoBehaviour, binding.Script.CastTo<MonoScript>());
			AddFloatKeyframe(curve, time, value);
		}

		private void AddEngineCurve(GenericBinding binding, string path, float time, float value)
		{
#warning TODO:
			FloatCurve curve = new FloatCurve(path, TypeTreePropertyPrefix + binding.Attribute, binding.ClassID, default);
			AddFloatKeyframe(curve, time, value);
		}

		private void AddAnimatorMuscleCurve(GenericBinding binding, float time, float value)
		{
			FloatCurve curve = new FloatCurve(string.Empty, binding.GetHumanoidMuscle(Layout.Info.Version).ToAttributeString(), ClassIDType.Animator, default);
			AddFloatKeyframe(curve, time, value);
		}

		private void AddFloatKeyframe(FloatCurve curve, float time, float value)
		{
			if (!m_floats.TryGetValue(curve, out List<KeyframeTpl<Float>> floatCurve))
			{
				floatCurve = new List<KeyframeTpl<Float>>();
				m_floats.Add(curve, floatCurve);
			}

			KeyframeTpl<Float> floatKey = new KeyframeTpl<Float>(time, value, KeyframeTpl<Float>.DefaultFloatWeight);
			floatCurve.Add(floatKey);
		}

		private void AddPPtrKeyframe(PPtrCurve curve, AnimationClipBindingConstant bindings, float time, int index)
		{
			if (!m_pptrs.TryGetValue(curve, out List<PPtrKeyframe> pptrCurve))
			{
				pptrCurve = new List<PPtrKeyframe>();
				m_pptrs.Add(curve, pptrCurve);
				AddPPtrKeyframe(curve, bindings, 0.0f, index - 1);
			}

			PPtr<Object> value = bindings.PPtrCurveMapping[index];
			PPtrKeyframe pptrKey = new PPtrKeyframe(time, value);
			pptrCurve.Add(pptrKey);
		}

		private void GetPreviousFrame(IReadOnlyList<StreamedFrame> streamFrames, int curveID, int currentFrame, out int frameIndex, out int curveIndex)
		{
			for (frameIndex = currentFrame - 1; frameIndex >= 0; frameIndex--)
			{
				StreamedFrame frame = streamFrames[frameIndex];
				for (curveIndex = 0; curveIndex < frame.Curves.Length; curveIndex++)
				{
					StreamedCurveKey curve = frame.Curves[curveIndex];
					if (curve.Index == curveID)
					{
						return;
					}
				}
			}
			throw new Exception($"There is no curve with index {curveID} in any of previous frames");
		}

		private int GetNextCurve(StreamedFrame frame, int currentCurve)
		{
			StreamedCurveKey curve = frame.Curves[currentCurve];
			int i = currentCurve + 1;
			for (; i < frame.Curves.Length; i++)
			{
				if (frame.Curves[i].Index != curve.Index)
				{
					return i;
				}
			}
			return i;
		}

		private static string GetCurvePath(IReadOnlyDictionary<uint, string> tos, uint hash)
		{
			if (tos.TryGetValue(hash, out string path))
			{
				return path;
			}
			else
			{
				return UnknownPathPrefix + hash;
			}
		}

		public Vector3Curve[] Translations { get; private set; }
		public QuaternionCurve[] Rotations { get; private set; }
		public Vector3Curve[] Scales { get; private set; }
		public Vector3Curve[] Eulers { get; private set; }
		public FloatCurve[] Floats { get; private set; }
		public PPtrCurve[] PPtrs { get; private set; }

		private AssetLayout Layout => m_clip.File.Layout;

		public static readonly Regex UnknownPathRegex = new Regex($@"^{UnknownPathPrefix}[0-9]{{1,10}}$", RegexOptions.Compiled);

		private const string UnknownPathPrefix = "path_";
		private const string MissedPropertyPrefix = "missed_";
		private const string ScriptPropertyPrefix = "script_";
		private const string TypeTreePropertyPrefix = "typetree_";

		private readonly Dictionary<Vector3Curve, List<KeyframeTpl<Vector3f>>> m_translations = new Dictionary<Vector3Curve, List<KeyframeTpl<Vector3f>>>();
		private readonly Dictionary<QuaternionCurve, List<KeyframeTpl<Quaternionf>>> m_rotations = new Dictionary<QuaternionCurve, List<KeyframeTpl<Quaternionf>>>();
		private readonly Dictionary<Vector3Curve, List<KeyframeTpl<Vector3f>>> m_scales = new Dictionary<Vector3Curve, List<KeyframeTpl<Vector3f>>>();
		private readonly Dictionary<Vector3Curve, List<KeyframeTpl<Vector3f>>> m_eulers = new Dictionary<Vector3Curve, List<KeyframeTpl<Vector3f>>>();
		private readonly Dictionary<FloatCurve, List<KeyframeTpl<Float>>> m_floats = new Dictionary<FloatCurve, List<KeyframeTpl<Float>>>();
		private readonly Dictionary<PPtrCurve, List<PPtrKeyframe>> m_pptrs = new Dictionary<PPtrCurve, List<PPtrKeyframe>>();

		private readonly AnimationClip m_clip;
		private readonly CustomCurveResolver m_customCurveResolver;
	}
}
