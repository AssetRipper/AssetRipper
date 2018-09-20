using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes.AnimationClips.Editor;

namespace uTinyRipper.Classes.AnimationClips
{
	public class AnimationClipGenericConverter
	{
		public AnimationClipGenericConverter(Version version, Platform platform, TransferInstructionFlags flags)
		{
			m_version = version;
			m_platform = platform;
			m_flags = flags;
		}

		public void Process(Clip clip, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos)
		{
			IReadOnlyList<StreamedFrame> streamedFrames = clip.StreamedClip.GenerateFrames(m_version, m_platform, m_flags);
			float lastDenseFrame = clip.DenseClip.FrameCount / clip.DenseClip.SampleRate;
			float lastSampleFrame = streamedFrames.Count > 1 ? streamedFrames[streamedFrames.Count - 2].Time : 0.0f;
			float lastFrame = Math.Max(lastDenseFrame, lastSampleFrame);

			Clear();
			ProcessStreams(streamedFrames, bindings, tos);
			ProcessDenses(clip, bindings, tos);
			if(Clip.IsReadConstantClip(m_version))
			{
				ProcessConstant(clip, bindings, tos, lastFrame);
			}
			CreateCurves();
		}

		public void Clear()
		{
			m_translationCurves = null;
			m_rotationCurves = null;
			m_scaleCurves = null;
			m_eulerCurves = null;
			m_floatCurves = null;

			m_translations.Clear();
			m_rotations.Clear();
			m_scales.Clear();
			m_eulers.Clear();
			m_floats.Clear();
		}

		private void CreateCurves()
		{
			m_translationCurves = m_translations.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
			m_rotationCurves = m_rotations.Select(t => new QuaternionCurve(t.Key, t.Value)).ToArray();
			m_scaleCurves = m_scales.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
			m_eulerCurves = m_eulers.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
			m_floatCurves = m_floats.Select(t => new FloatCurve(t.Key, t.Value)).ToArray();
		}

		private void ProcessStreams(IReadOnlyList<StreamedFrame> streamFrames, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos)
		{
			float[] curveValues = new float[4];
			float[] inSlopeValues = new float[4];
			float[] outSlopeValues = new float[4];

			// first (index [0]) stream frame is for slope calculation for the first real frame (index [1])
			// last one (index [count - 1]) is +Infinity
			// it is made for slope processing, but we don't need them
			for (int frameIndex = 1; frameIndex < streamFrames.Count - 1; frameIndex++)
			{
				StreamedFrame frame = streamFrames[frameIndex];
				for (int curveIndex = 0; curveIndex < frame.Curves.Count; )
				{
					StreamedCurveKey curve = frame.Curves[curveIndex];
					if (!GetGenericBinding(bindings, curve.Index, out GenericBinding binding))
					{
						curveIndex++;
						continue;
					}

					//FindPreviousCurve(streamFrames, curve.Index, frameIndex, out int prevFrameIndex, out int prevCurveIndex);
					//FindNextCurve(streamFrames, curve.Index, frameIndex, out int nextFrameIndex, out int nextCurveIndex);

					string path = GetCurvePath(tos, binding.Path);
					int dimension = binding.BindingType.GetDimension();
					for (int key = 0; key < dimension; key++)
					{
						StreamedCurveKey keyCurve = frame.Curves[curveIndex + key];
						//StreamedFrame prevFrame = streamFrames[prevFrameIndex];
						//StreamedFrame nextFrame = streamFrames[nextFrameIndex];
						//StreamedCurveKey prevKeyCurve = prevFrame.Curves[prevCurveIndex + key];
						//StreamedCurveKey nextKeyCurve = nextFrame.Curves[nextCurveIndex + key];
						curveValues[key] = keyCurve.Value;
#warning TODO: TCB to in/out slope
						//inSlopeValues[key] = prevKeyCurve.CalculateNextInTangent(keyCurve.Value, nextKeyCurve.Value);
						//outSlopeValues[key] = keyCurve.CalculateOutTangent(prevKeyCurve.Value, nextKeyCurve.Value);
					}

					AddComplexCurve(frame.Time, binding.BindingType, curveValues, inSlopeValues, outSlopeValues, 0, path);
					curveIndex += dimension;
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
					if (!GetGenericBinding(bindings, index, out GenericBinding binding))
					{
						curveIndex++;
						continue;
					}

					string path = GetCurvePath(tos, binding.Path);
					AddComplexCurve(time, binding.BindingType, dense.SampleArray, slopeValues, slopeValues, frameOffset + curveIndex, path);
					curveIndex += binding.BindingType.GetDimension();
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
				for (int curveIndex = 0; curveIndex < constant.Constants.Count;)
				{
					int index = streamCount + denseCount + curveIndex;
					if (!GetGenericBinding(bindings, index, out GenericBinding binding))
					{
						curveIndex++;
						continue;
					}

					string path = GetCurvePath(tos, binding.Path);
					AddComplexCurve(time, binding.BindingType, constant.Constants, slopeValues, slopeValues, curveIndex, path);
					curveIndex += binding.BindingType.GetDimension();
				}
			}
		}

		private void AddComplexCurve(float time, BindingType bindType, IReadOnlyList<float> curveValues,
			IReadOnlyList<float> inSlopeValues, IReadOnlyList<float> outSlopeValues, int offset, string path)
		{
			switch (bindType)
			{
				case BindingType.Translation:
					{
						if (!m_translations.TryGetValue(path, out List<KeyframeTpl<Vector3f>> transCurve))
						{
							transCurve = new List<KeyframeTpl<Vector3f>>();
							m_translations.Add(path, transCurve);
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
						Vector3f defWeight = new Vector3f(1.0f / 3.0f);
						KeyframeTpl<Vector3f> transKey = new KeyframeTpl<Vector3f>(time, value, inSlope, outSlope, defWeight);
						transCurve.Add(transKey);
						m_translations[path] = transCurve;
					}
					break;

				case BindingType.Rotation:
					{
						if (!m_rotations.TryGetValue(path, out List<KeyframeTpl<Quaternionf>> rotCurve))
						{
							rotCurve = new List<KeyframeTpl<Quaternionf>>();
							m_rotations.Add(path, rotCurve);
						}

						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];
						float w = curveValues[offset + 3];

						float inX = 0;//inSlopeValues[0];
						float inY = 0;//inSlopeValues[1];
						float inZ = 0;//inSlopeValues[2];
						float inW = 0;//inSlopeValues[3];

						float outX = 0;//outSlopeValues[0];
						float outY = 0;//outSlopeValues[1];
						float outZ = 0;//outSlopeValues[2];
						float outW = 0;//outSlopeValues[3];

						Quaternionf value = new Quaternionf(x, y, z, w);
						Quaternionf inSlope = new Quaternionf(inX, inY, inZ, inW);
						Quaternionf outSlope = new Quaternionf(outX, outY, outZ, outW);
						Quaternionf defWeight = new Quaternionf(1.0f / 3.0f);
						KeyframeTpl<Quaternionf> rotKey = new KeyframeTpl<Quaternionf>(time, value, inSlope, outSlope, defWeight);
						rotCurve.Add(rotKey);
						m_rotations[path] = rotCurve;
					}
					break;

				case BindingType.Scaling:
					{
						if (!m_scales.TryGetValue(path, out List<KeyframeTpl<Vector3f>> scaleCurve))
						{
							scaleCurve = new List<KeyframeTpl<Vector3f>>();
							m_scales.Add(path, scaleCurve);
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
						Vector3f defWeight = new Vector3f(1.0f / 3.0f);
						KeyframeTpl<Vector3f> scaleKey = new KeyframeTpl<Vector3f>(time, value, inSlope, outSlope, defWeight);
						scaleCurve.Add(scaleKey);
						m_scales[path] = scaleCurve;
					}
					break;

				case BindingType.EulerRotation:
					{
						if (!m_eulers.TryGetValue(path, out List<KeyframeTpl<Vector3f>> eulerCurve))
						{
							eulerCurve = new List<KeyframeTpl<Vector3f>>();
							m_eulers.Add(path, eulerCurve);
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
						Vector3f defWeight = new Vector3f(1.0f / 3.0f);
						KeyframeTpl<Vector3f> eulerKey = new KeyframeTpl<Vector3f>(time, value, inSlope, outSlope, defWeight);
						eulerCurve.Add(eulerKey);
						m_eulers[path] = eulerCurve;
					}
					break;

				case BindingType.Floats:
					{
						if (!m_floats.TryGetValue(path, out List<KeyframeTpl<Float>> floatCurve))
						{
							floatCurve = new List<KeyframeTpl<Float>>();
							m_floats.Add(path, floatCurve);
						}

						float x = curveValues[offset];
						float inX = inSlopeValues[0];
						float outX = outSlopeValues[0];

						Float value = new Float(x);
						Float inSlope = new Float(inX);
						Float outSlope = new Float(outX);
						Float defWeight = new Float(1.0f / 3.0f);
						KeyframeTpl<Float> floatKey = new KeyframeTpl<Float>(time, value, inSlope, outSlope, defWeight);
						floatCurve.Add(floatKey);
						m_floats[path] = floatCurve;
					}
					break;

				default:
					throw new NotImplementedException(bindType.ToString());
			}
		}

		private void FindPreviousCurve(IReadOnlyList<StreamedFrame> streamFrames, int index, int currentFrame, out int frameIndex, out int curveIndex)
		{
			for (frameIndex = currentFrame - 1; frameIndex >= 0; frameIndex--)
			{
				StreamedFrame frame = streamFrames[frameIndex];
				for (curveIndex = 0; curveIndex < frame.Curves.Count; curveIndex++)
				{
					StreamedCurveKey curve = frame.Curves[curveIndex];
					if (curve.Index == index)
					{
						return;
					}
				}
			}
			throw new Exception($"There is no curve with index {index} in any of previous frames");
		}

		private void FindNextCurve(IReadOnlyList<StreamedFrame> streamFrames, int index, int currentFrameIndex, out int frameIndex, out int curveIndex)
		{
			for (frameIndex = currentFrameIndex + 1; frameIndex < streamFrames.Count; frameIndex++)
			{
				StreamedFrame frame = streamFrames[frameIndex];
				for (curveIndex = 0; curveIndex < frame.Curves.Count; curveIndex++)
				{
					StreamedCurveKey curve = frame.Curves[curveIndex];
					if (curve.Index == index)
					{
						return;
					}
				}
			}

			// if there is no next curve, use current one
			frameIndex = currentFrameIndex;
			StreamedFrame currentFrame = streamFrames[currentFrameIndex];
			for (curveIndex = 0; curveIndex < currentFrame.Curves.Count; curveIndex++)
			{
				StreamedCurveKey curve = currentFrame.Curves[curveIndex];
				if (curve.Index == index)
				{
					return;
				}
			}
			throw new Exception($"There is no curve with index {index} in any of current or next frames");
		}

		private static bool GetGenericBinding(AnimationClipBindingConstant bindings, int index, out GenericBinding binding)
		{
			binding = bindings.FindBinding(index);
			if (binding.ClassID == ClassIDType.Transform)
			{
				return true;
			}
#warning TODO: humanoid
			return false;
		}

		private static string GetCurvePath(IReadOnlyDictionary<uint, string> tos, uint hash)
		{
			if (tos.TryGetValue(hash, out string path))
			{
				return path;
			}
			else
			{
				return "dummy_" + hash;
			}
		}

		public IReadOnlyList<Vector3Curve> Translations => m_translationCurves;
		public IReadOnlyList<QuaternionCurve> Rotations => m_rotationCurves;
		public IReadOnlyList<Vector3Curve> Scales => m_scaleCurves;
		public IReadOnlyList<Vector3Curve> Eulers => m_eulerCurves;
		public IReadOnlyList<FloatCurve> Floats => m_floatCurves;

		private readonly Dictionary<string, List<KeyframeTpl<Vector3f>>> m_translations = new Dictionary<string, List<KeyframeTpl<Vector3f>>>();
		private readonly Dictionary<string, List<KeyframeTpl<Quaternionf>>> m_rotations = new Dictionary<string, List<KeyframeTpl<Quaternionf>>>();
		private readonly Dictionary<string, List<KeyframeTpl<Vector3f>>> m_scales = new Dictionary<string, List<KeyframeTpl<Vector3f>>>();
		private readonly Dictionary<string, List<KeyframeTpl<Vector3f>>> m_eulers = new Dictionary<string, List<KeyframeTpl<Vector3f>>>();
		private readonly Dictionary<string, List<KeyframeTpl<Float>>> m_floats = new Dictionary<string, List<KeyframeTpl<Float>>>();

		private readonly Version m_version;
		private readonly Platform m_platform;
		private readonly TransferInstructionFlags m_flags;

		private Vector3Curve[] m_translationCurves;
		private QuaternionCurve[] m_rotationCurves;
		private Vector3Curve[] m_scaleCurves;
		private Vector3Curve[] m_eulerCurves;
		private FloatCurve[] m_floatCurves;
	}
}
