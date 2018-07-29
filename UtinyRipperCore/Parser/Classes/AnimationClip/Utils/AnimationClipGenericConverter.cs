using System;
using System.Collections.Generic;
using UtinyRipper.Classes.AnimationClips.Editor;

namespace UtinyRipper.Classes.AnimationClips
{
	public class AnimationClipGenericConverter
	{
		public AnimationClipGenericConverter(Version version, Platform platform)
		{
			m_version = version;
			m_platform = platform;
		}

		public void Process(Clip clip, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos)
		{
			IReadOnlyList<StreamedFrame> streamedFrames = clip.StreamedClip.GenerateFrames(m_version, m_platform);
			int frameCount = Math.Max(clip.DenseClip.FrameCount, streamedFrames.Count - 2);

			ProcessStreams(streamedFrames, bindings, tos);
			ProcessDenses(clip, bindings, tos);
			ProcessConstant(clip, bindings, tos, frameCount);
		}

#warning TODO: read TCB and convert to in/out slope
		private void ProcessStreams(IReadOnlyList<StreamedFrame> streamFrames, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos)
		{
			float[] curveValues = new float[4];
			// first (index [0]) stream frame is -Infinity
			// last one is +Infinity
			// it is made for slope processing, but we don't need them
			for (int frameIndex = 1; frameIndex < streamFrames.Count - 1; frameIndex++)
			{
				StreamedFrame frame = streamFrames[frameIndex];
				for (int curveIndex = 0; curveIndex < frame.Curves.Count; )
				{
					StreamedCurveKey curve = frame.Curves[curveIndex];
					if(!GetGenericBinding(bindings, curve.Index, out GenericBinding binding))
					{
						curveIndex++;
						continue;
					}

					string path = GetCurvePath(tos, binding.Path);
					int dimension = binding.BindingType.GetDimension();
					for (int key = 0; key < dimension; key++)
					{
						curveValues[key] = frame.Curves[curveIndex + key].Value;
					}

					AddComplexCurve(frame.Time, binding.BindingType, curveValues, 0, path);
					curveIndex += dimension;
				}
			}
		}

		private void ProcessDenses(Clip clip, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos)
		{
			DenseClip dense = clip.DenseClip;
			int streamCount = clip.StreamedClip.CurveCount;
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
					AddComplexCurve(time, binding.BindingType, dense.SampleArray, frameOffset + curveIndex, path);
					curveIndex += binding.BindingType.GetDimension();
				}
			}
		}

		private void ProcessConstant(Clip clip, AnimationClipBindingConstant bindings, IReadOnlyDictionary<uint, string> tos, int frameCount)
		{
			DenseClip dense = clip.DenseClip;
			ConstantClip constant = clip.ConstantClip;
			int streamCount = clip.StreamedClip.CurveCount;
			int denseCount = clip.DenseClip.CurveCount;

			// only first and last frames
			for (int frameIndex = 0; frameIndex < frameCount; frameIndex += (frameCount > 1 ? frameCount - 1 : 1))
			{
				float time = frameIndex / dense.SampleRate;
				for (int curveIndex = 0; curveIndex < constant.Constants.Count;)
				{
					int index = streamCount + denseCount + curveIndex;
					if (!GetGenericBinding(bindings, index, out GenericBinding binding))
					{
						curveIndex++;
						continue;
					}

					string path = GetCurvePath(tos, binding.Path);
					AddComplexCurve(time, binding.BindingType, constant.Constants, curveIndex, path);
					curveIndex += binding.BindingType.GetDimension();
				}
			}
		}

		private void AddComplexCurve(float time, BindingType bindType, IReadOnlyList<float> curveValues, int offset, string path)
		{
			switch (bindType)
			{
				case BindingType.Translation:
					{
						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];

						if (!m_translations.TryGetValue(path, out Vector3Curve transCurve))
						{
							transCurve = new Vector3Curve(path);
						}

						Vector3f trans = new Vector3f(x, y, z);
						Vector3f defWeight = new Vector3f(1.0f / 3.0f);
						KeyframeTpl<Vector3f> transKey = new KeyframeTpl<Vector3f>(time, trans, defWeight);
						transCurve.Curve.Curve.Add(transKey);
						m_translations[path] = transCurve;
					}
					break;

				case BindingType.Rotation:
					{
						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];
						float w = curveValues[offset + 3];

						if (!m_rotations.TryGetValue(path, out QuaternionCurve rotCurve))
						{
							rotCurve = new QuaternionCurve(path);
						}

						Quaternionf rot = new Quaternionf(x, y, z, w);
						Quaternionf defWeight = new Quaternionf(1.0f / 3.0f);
						KeyframeTpl<Quaternionf> rotKey = new KeyframeTpl<Quaternionf>(time, rot, defWeight);
						rotCurve.Curve.Curve.Add(rotKey);
						m_rotations[path] = rotCurve;
					}
					break;

				case BindingType.Scaling:
					{
						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];
						
						if (!m_scales.TryGetValue(path, out Vector3Curve scaleCurve))
						{
							scaleCurve = new Vector3Curve(path);
						}

						Vector3f scale = new Vector3f(x, y, z);
						Vector3f defWeight = new Vector3f(1.0f / 3.0f);
						KeyframeTpl<Vector3f> scaleKey = new KeyframeTpl<Vector3f>(time, scale, defWeight);
						scaleCurve.Curve.Curve.Add(scaleKey);
						m_scales[path] = scaleCurve;
					}
					break;

				case BindingType.EulerRotation:
					{
						float x = curveValues[offset + 0];
						float y = curveValues[offset + 1];
						float z = curveValues[offset + 2];

						if (!m_eulers.TryGetValue(path, out Vector3Curve eulerCurve))
						{
							eulerCurve = new Vector3Curve(path);
						}

						Vector3f euler = new Vector3f(x, y, z);
						Vector3f defWeight = new Vector3f(1.0f / 3.0f);
						KeyframeTpl<Vector3f> eulerKey = new KeyframeTpl<Vector3f>(time, euler, defWeight);
						eulerCurve.Curve.Curve.Add(eulerKey);
						m_eulers[path] = eulerCurve;
					}
					break;

				case BindingType.Floats:
					{
						float value = curveValues[offset];

						if (!m_floats.TryGetValue(path, out FloatCurve floatCurve))
						{
							floatCurve = new FloatCurve(path);
						}

						Float @float = new Float(value);
						Float defWeight = new Float(1.0f / 3.0f);
						KeyframeTpl<Float> floatKey = new KeyframeTpl<Float>(time, @float, defWeight);
						floatCurve.Curve.Curve.Add(floatKey);
						m_floats[path] = floatCurve;
					}
					break;

				default:
					throw new NotImplementedException(bindType.ToString());
			}
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

		public IReadOnlyDictionary<string, Vector3Curve> Translations => m_translations;
		public IReadOnlyDictionary<string, QuaternionCurve> Rotations => m_rotations;
		public IReadOnlyDictionary<string, Vector3Curve> Scales => m_scales;
		public IReadOnlyDictionary<string, Vector3Curve> Eulers => m_eulers;
		public IReadOnlyDictionary<string, FloatCurve> Floats => m_floats;

		private readonly Dictionary<string, Vector3Curve> m_translations = new Dictionary<string, Vector3Curve>();
		private readonly Dictionary<string, QuaternionCurve> m_rotations = new Dictionary<string, QuaternionCurve>();
		private readonly Dictionary<string, Vector3Curve> m_scales = new Dictionary<string, Vector3Curve>();
		private readonly Dictionary<string, Vector3Curve> m_eulers = new Dictionary<string, Vector3Curve>();
		private readonly Dictionary<string, FloatCurve> m_floats = new Dictionary<string, FloatCurve>();

		private readonly Version m_version;
		private readonly Platform m_platform;
	}
}
