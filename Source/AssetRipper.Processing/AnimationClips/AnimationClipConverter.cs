using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Checksum;
using AssetRipper.Processing.AnimationClips.Editor;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.GenericBinding;
using AssetRipper.SourceGenerated.Extensions.Enums.Keyframe.TangentMode;
using AssetRipper.SourceGenerated.Subclasses.AnimationClipBindingConstant;
using AssetRipper.SourceGenerated.Subclasses.Clip;
using AssetRipper.SourceGenerated.Subclasses.ConstantClip;
using AssetRipper.SourceGenerated.Subclasses.DenseClip;
using AssetRipper.SourceGenerated.Subclasses.FloatCurve;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Quaternionf;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Single;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Vector3f;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;
using AssetRipper.SourceGenerated.Subclasses.PPtrCurve;
using AssetRipper.SourceGenerated.Subclasses.PPtrKeyframe;
using AssetRipper.SourceGenerated.Subclasses.QuaternionCurve;
using AssetRipper.SourceGenerated.Subclasses.StreamedClip;
using AssetRipper.SourceGenerated.Subclasses.Vector3Curve;
using System.Buffers;
using System.Runtime.InteropServices;

namespace AssetRipper.Processing.AnimationClips
{
	public readonly partial struct AnimationClipConverter
	{
		private AnimationClipConverter(IAnimationClip clip, PathChecksumCache checksumCache)
		{
			m_clip = clip;
			m_customCurveResolver = new CustomCurveResolver(clip);
			m_checksumCache = checksumCache;
		}

		public static void Process(IAnimationClip clip, PathChecksumCache checksumCache)
		{
			AnimationClipConverter converter = new AnimationClipConverter(clip, checksumCache);
			converter.ProcessInner();
		}

		private void ProcessInner()
		{
			if (m_clip.Has_MuscleClip_C74() && m_clip.Has_ClipBindingConstant_C74())
			{
				IClip clip = m_clip.MuscleClip_C74.Clip.Data;
				IAnimationClipBindingConstant bindings = m_clip.ClipBindingConstant_C74;

				IReadOnlyList<StreamedFrame> streamedFrames = GenerateFramesFromStreamedClip(clip.StreamedClip);
				float lastDenseFrame = clip.DenseClip.FrameCount / clip.DenseClip.SampleRate;
				float lastSampleFrame = streamedFrames.Count > 1 ? streamedFrames[streamedFrames.Count - 2].Time : 0.0f;
				float lastFrame = Math.Max(lastDenseFrame, lastSampleFrame);

				ProcessStreams(streamedFrames, bindings, clip.DenseClip.SampleRate);
				ProcessDenses(clip, bindings);
				if (clip.Has_ConstantClip())
				{
					ProcessConstant(clip, clip.ConstantClip, bindings, lastFrame);
				}
				if (m_clip.Has_MuscleClipInfo_C74())
				{
					m_clip.MuscleClipInfo_C74.Initialize(m_clip.MuscleClip_C74);
				}
			}
		}

		private void ProcessStreams(IReadOnlyList<StreamedFrame> streamFrames, IAnimationClipBindingConstant bindings, float sampleRate)
		{
			Span<float> curveValues = stackalloc float[4] { 0, 0, 0, 0 };
			Span<float> inSlopeValues = stackalloc float[4] { 0, 0, 0, 0 };
			Span<float> outSlopeValues = stackalloc float[4] { 0, 0, 0, 0 };
			float interval = 1.0f / sampleRate;

			// first (index [0]) stream frame is for slope calculation for the first real frame (index [1])
			// last one (index [count - 1]) is +Infinity
			// it is made for slope processing, but we don't need them
			bool frameIndex0 = true;
			for (int frameIndex = 0; frameIndex < streamFrames.Count - 1; frameIndex++)
			{
				StreamedFrame frame = streamFrames[frameIndex];
				for (int curveIndex = 0; curveIndex < frame.Curves.Length;)
				{
					StreamedCurveKey curve = frame.Curves[curveIndex];
					IGenericBinding binding = bindings.FindBinding(curve.Index);

					string path = GetCurvePath(binding.Path);
					if (binding.IsTransform())
					{
						if (frameIndex0)
						{
							curveIndex = GetNextCurve(frame, curveIndex);
							continue;
						}
						GetPreviousFrame(streamFrames, curve.Index, frameIndex, out int prevFrameIndex, out int prevCurveIndex);
						int dimension = binding.TransformType().GetDimension();
						for (int key = 0; key < dimension; key++)
						{
							StreamedCurveKey keyCurve = frame.Curves[curveIndex];//index out of bounds
							StreamedFrame prevFrame = streamFrames[prevFrameIndex];
							StreamedCurveKey prevKeyCurve = prevFrame.Curves[prevCurveIndex + key];
							float deltaTime = frame.Time - prevFrame.Time;
							curveValues[key] = keyCurve.Value;
							inSlopeValues[key] = prevKeyCurve.CalculateNextInSlope(deltaTime, keyCurve.Value);
							outSlopeValues[key] = keyCurve.OutSlope;
							curveIndex = GetNextCurve(frame, curveIndex);
						}

						AddTransformCurve(frame.Time, binding.TransformType(), curveValues, inSlopeValues, outSlopeValues, 0, path);
					}
					else if (binding.CustomType == (byte)BindingCustomType.None)
					{
						if (frameIndex0)
						{
							curveIndex = GetNextCurve(frame, curveIndex);
							continue;
						}
						AddDefaultCurve(binding, path, frame.Time, frame.Curves[curveIndex].Value);
						curveIndex = GetNextCurve(frame, curveIndex);
					}
					else
					{
						AddCustomCurve(bindings, binding, path, frame.Time, frame.Curves[curveIndex].Value);
						curveIndex = GetNextCurve(frame, curveIndex);
					}
				}
				if (frameIndex0)
				{
					frameIndex0 = false;
				}
			}
		}

		private void ProcessDenses(IClip clip, IAnimationClipBindingConstant bindings)
		{
			DenseClip dense = clip.DenseClip;

			float[] rentedArray = ArrayPool<float>.Shared.Rent(dense.SampleArray.Count);
			dense.SampleArray.CopyTo(rentedArray);
			ReadOnlySpan<float> curveValues = new ReadOnlySpan<float>(rentedArray, 0, dense.SampleArray.Count);

			ReadOnlySpan<float> slopeValues = stackalloc float[4] { 0, 0, 0, 0 }; // no slopes - 0 values

			int streamCount = (int)clip.StreamedClip.CurveCount;
			for (int frameIndex = 0; frameIndex < dense.FrameCount; frameIndex++)
			{
				float time = frameIndex / dense.SampleRate;
				int frameOffset = frameIndex * (int)dense.CurveCount;
				for (int curveIndex = 0; curveIndex < dense.CurveCount;)
				{
					int index = streamCount + curveIndex;
					IGenericBinding binding = bindings.FindBinding(index);
					string path = GetCurvePath(binding.Path);
					int framePosition = frameOffset + curveIndex;
					if (binding.IsTransform())
					{
						AddTransformCurve(time, binding.TransformType(), curveValues, slopeValues, slopeValues, framePosition, path);
						curveIndex += binding.TransformType().GetDimension();
					}
					else if (binding.CustomType == (byte)BindingCustomType.None)
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
			ArrayPool<float>.Shared.Return(rentedArray);
		}

		private void ProcessConstant(IClip clip, IConstantClip constant, IAnimationClipBindingConstant bindings, float lastFrame)
		{
			float[] rentedArray = ArrayPool<float>.Shared.Rent(constant.Data.Count);
			constant.Data.CopyTo(rentedArray);
			ReadOnlySpan<float> curveValues = new ReadOnlySpan<float>(rentedArray, 0, constant.Data.Count);

			ReadOnlySpan<float> slopeValues = stackalloc float[4] { 0, 0, 0, 0 }; // no slopes - 0 values

			int streamCount = (int)clip.StreamedClip.CurveCount;
			int denseCount = (int)clip.DenseClip.CurveCount;

			// only first and last frames
			float time = 0.0f;
			for (int i = 0; i < 2; i++, time += lastFrame)
			{
				for (int curveIndex = 0; curveIndex < constant.Data.Count;)
				{
					int index = streamCount + denseCount + curveIndex;
					IGenericBinding binding = bindings.FindBinding(index);
					string path = GetCurvePath(binding.Path);
					if (binding.IsTransform())
					{
						AddTransformCurve(time, binding.TransformType(), curveValues, slopeValues, slopeValues, curveIndex, path);
						curveIndex += binding.TransformType().GetDimension();
					}
					else if (binding.CustomType == (byte)BindingCustomType.None)
					{
						AddDefaultCurve(binding, path, time, constant.Data[curveIndex]);
						curveIndex++;
					}
					else
					{
						AddCustomCurve(bindings, binding, path, time, constant.Data[curveIndex]);
						curveIndex++;
					}
				}
			}
			ArrayPool<float>.Shared.Return(rentedArray);
		}

		private void AddCustomCurve(IAnimationClipBindingConstant bindings, IGenericBinding binding, string path, float time, float value)
		{
			bool ProcessStreams_frameIndex0 = time != FrameIndex0Time;
			switch ((BindingCustomType)binding.CustomType)
			{
				case BindingCustomType.AnimatorMuscle:
					if (ProcessStreams_frameIndex0)
					{
						AddAnimatorMuscleCurve(binding, time, value);
					}

					break;

				default:
					string attribute = m_customCurveResolver.ToAttributeName((BindingCustomType)binding.CustomType, binding.Attribute, path);
					if (binding.IsPPtrCurve())
					{
						if (!ProcessStreams_frameIndex0)
						{
							time = 0.0f;
						}

						CurveData curve = new CurveData(path, attribute, binding.GetClassID(), binding.Script.TryGetAsset(m_clip.Collection));
						AddPPtrKeyframe(curve, bindings, time, (int)value);
					}
					else if (ProcessStreams_frameIndex0)
					{
						CurveData curve = new CurveData(path, attribute, binding.GetClassID(), binding.Script.TryGetAsset(m_clip.Collection));
						AddFloatKeyframe(curve, time, value);
					}
					break;
			}
		}

		private void AddTransformCurve(float time, TransformType transType, ReadOnlySpan<float> curveValues,
			ReadOnlySpan<float> inSlopeValues, ReadOnlySpan<float> outSlopeValues, int offset, string path)
		{
			switch (transType)
			{
				case TransformType.Translation:
					{
						if (!m_translations.TryGetValue(path, out IVector3Curve? curve))
						{
							curve = m_clip.PositionCurves_C74.AddNew();
							curve.SetValues(path);
							m_translations.Add(path, curve);
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

						IKeyframe_Vector3f key = curve.Curve.Curve.AddNew();

						key.Value.SetValues(x, y, z);
						key.InSlope.SetValues(inX, inY, inZ);
						key.OutSlope.SetValues(outX, outY, outZ);
						key.Time = time;
						// this enum member is version agnostic
						key.TangentMode = TangentMode.FreeFree.ToTangent(Version);
						key.WeightedMode = (int)WeightedMode.None;
						key.InWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
						key.OutWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
					}
					break;

				case TransformType.Rotation:
					{
						if (!m_rotations.TryGetValue(path, out IQuaternionCurve? curve))
						{
							curve = m_clip.RotationCurves_C74.AddNew();
							curve.SetValues(path);
							m_rotations.Add(path, curve);
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

						IKeyframe_Quaternionf key = curve.Curve.Curve.AddNew();

						key.Value.SetValues(x, y, z, w);
						key.InSlope.SetValues(inX, inY, inZ, inW);
						key.OutSlope.SetValues(outX, outY, outZ, outW);
						key.Time = time;
						// this enum member is version agnostic
						key.TangentMode = TangentMode.FreeFree.ToTangent(Version);
						key.WeightedMode = (int)WeightedMode.None;
						key.InWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
						key.OutWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
					}
					break;

				case TransformType.Scaling:
					{
						if (!m_scales.TryGetValue(path, out IVector3Curve? curve))
						{
							curve = m_clip.ScaleCurves_C74.AddNew();
							curve.SetValues(path);
							m_scales.Add(path, curve);
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

						IKeyframe_Vector3f key = curve.Curve.Curve.AddNew();

						key.Value.SetValues(x, y, z);
						key.InSlope.SetValues(inX, inY, inZ);
						key.OutSlope.SetValues(outX, outY, outZ);
						key.Time = time;
						// this enum member is version agnostic
						key.TangentMode = TangentMode.FreeFree.ToTangent(Version);
						key.WeightedMode = (int)WeightedMode.None;
						key.InWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
						key.OutWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
					}
					break;

				case TransformType.EulerRotation:
					{
						if (!m_eulers.TryGetValue(path, out IVector3Curve? curve))
						{
							if (!m_clip.Has_EulerCurves_C74())
							{
								break;
							}
							curve = m_clip.EulerCurves_C74.AddNew();
							curve.SetValues(path);
							m_eulers.Add(path, curve);
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

						IKeyframe_Vector3f key = curve.Curve.Curve.AddNew();

						key.Value.SetValues(x, y, z);
						key.InSlope.SetValues(inX, inY, inZ);
						key.OutSlope.SetValues(outX, outY, outZ);
						key.Time = time;
						// this enum member is version agnostic
						key.TangentMode = TangentMode.FreeFree.ToTangent(Version);
						key.WeightedMode = (int)WeightedMode.None;
						key.InWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
						key.OutWeight?.SetValues(DefaultFloatWeight, DefaultFloatWeight, DefaultFloatWeight);
					}
					break;

				default:
					throw new NotImplementedException(transType.ToString());
			}
		}

		private void AddDefaultCurve(IGenericBinding binding, string path, float time, float value)
		{
			switch (binding.GetClassID())
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

		private void AddGameObjectCurve(IGenericBinding binding, string path, float time, float value)
		{
			if (GameObject.TryGetPath(binding.Attribute, out string? propertyName))
			{
				CurveData curve = new CurveData(path, propertyName, ClassIDType.GameObject);
				AddFloatKeyframe(curve, time, value);
				return;
			}
			else
			{
				// that means that dev exported animation clip with missing component
				CurveData curve = new CurveData(path, MissedPropertyPrefix + binding.Attribute, ClassIDType.GameObject);
				AddFloatKeyframe(curve, time, value);
			}
		}

		private void AddScriptCurve(IGenericBinding binding, string path, float time, float value)
		{
			if (binding.Script.TryGetAsset(m_clip.Collection) is IMonoScript script)
			{
				m_checksumCache.Add(script);
			}

			if (!m_checksumCache.TryGetPath(binding.Attribute, out string? propertyName))
			{
				propertyName = ScriptPropertyPrefix + binding.Attribute;
			}

			CurveData curve = new CurveData(path, propertyName, ClassIDType.MonoBehaviour, binding.Script.TryGetAsset(m_clip.Collection));

			AddFloatKeyframe(curve, time, value);
		}

		private void AddEngineCurve(IGenericBinding binding, string path, float time, float value)
		{
			if (!FieldHashes.TryGetPath(binding.GetClassID(), binding.Attribute, out string? propertyName))
			{
				CurveData curve = new(path, TypeTreePropertyPrefix + binding.Attribute, binding.GetClassID());
				AddFloatKeyframe(curve, time, value);
			}
			else
			{
				CurveData curve = new(path, propertyName, binding.GetClassID());
				AddFloatKeyframe(curve, time, value);
			}
		}

		private void AddAnimatorMuscleCurve(IGenericBinding binding, float time, float value)
		{
			string attributeString = HumanoidMuscleTypeExtensions.ToAttributeString(binding.GetHumanoidMuscle(Version));
			CurveData curve = new CurveData(string.Empty, attributeString, ClassIDType.Animator);
			AddFloatKeyframe(curve, time, value);
		}

		private void AddFloatKeyframe(in CurveData curveData, float time, float value)
		{
			if (!m_floats.TryGetValue(curveData, out IFloatCurve? curve))
			{
				curve = m_clip.FloatCurves_C74.AddNew();
				curve.Path = curveData.Path;
				curve.Attribute = curveData.Attribute;
				curve.ClassID = (int)curveData.ClassID;
				curve.Script.SetAsset(m_clip.Collection, curveData.Script as IMonoScript);
				curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
				m_floats.Add(curveData, curve);
			}

			IKeyframe_Single floatKey = curve.Curve.Curve.AddNew();
			floatKey.SetValues(Version, time, value, DefaultFloatWeight);
		}

		private void AddPPtrKeyframe(in CurveData curveData, IAnimationClipBindingConstant bindings, float time, int index)
		{
			if (!m_pptrs.TryGetValue(curveData, out IPPtrCurve? curve))
			{
				if (!m_clip.Has_PPtrCurves_C74())
				{
					return;
				}
				curve = m_clip.PPtrCurves_C74.AddNew();
				curve.Path = curveData.Path;
				curve.Attribute = curveData.Attribute;
				curve.ClassID = (int)curveData.ClassID;
				curve.Script.SetAsset(m_clip.Collection, curveData.Script as IMonoScript);
				m_pptrs.Add(curveData, curve);
			}

			IPPtr_Object value = bindings.PptrCurveMapping[index];
			IPPtrKeyframe key = curve.Curve.AddNew();
			key.Time = time;
			key.Value.CopyValues(value, new PPtrConverter(m_clip));
		}

		private static void GetPreviousFrame(IReadOnlyList<StreamedFrame> streamFrames, int curveID, int currentFrame, out int frameIndex, out int curveIndex)
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

		private static int GetNextCurve(StreamedFrame frame, int currentCurve)
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

		private string GetCurvePath(uint hash)
		{
			if (m_checksumCache.TryGetPath(hash, out string? path))
			{
				return path;
			}
			else
			{
				return Crc32Algorithm.ReverseAscii(hash, $"{UnknownPathPrefix}0x{hash:X}_");
			}
		}

		public IReadOnlyList<StreamedFrame> GenerateFramesFromStreamedClip(StreamedClip clip)
		{
			List<StreamedFrame> frames = new();
			byte[] memStreamBuffer = new byte[clip.Data.Count * sizeof(uint)];
			{
				Span<uint> span = MemoryMarshal.Cast<byte, uint>(memStreamBuffer);
				clip.Data.CopyTo(span);
			}

			using MemoryStream stream = new MemoryStream(memStreamBuffer);
			using AssetReader reader = new AssetReader(stream, m_clip.Collection);
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				StreamedFrame frame = new();
				frame.Read(reader);
				frames.Add(frame);
			}
			return frames;
		}

		private UnityVersion Version => m_clip.Collection.Version;

		private const string UnknownPathPrefix = "path_";
		private const string MissedPropertyPrefix = "missed_";
		private const string ScriptPropertyPrefix = "script_";
		private const string TypeTreePropertyPrefix = "typetree_";

		/// <summary>
		/// Used to detect when a StreamedFrame is from index 0.
		/// </summary>
		private const float FrameIndex0Time = float.MinValue;

		/// <summary>
		/// The default weight for a keyframe.
		/// </summary>
		/// <remarks>
		/// The default Vector3 is 1/3, 1/3, 1/3
		/// The default Quaternion is 1/3, 1/3, 1/3, 1/3
		/// </remarks>
		private const float DefaultFloatWeight = 1.0f / 3.0f;

		private readonly Dictionary<string, IVector3Curve> m_translations = new();
		private readonly Dictionary<string, IQuaternionCurve> m_rotations = new();
		private readonly Dictionary<string, IVector3Curve> m_scales = new();
		private readonly Dictionary<string, IVector3Curve> m_eulers = new();
		private readonly Dictionary<CurveData, IFloatCurve> m_floats = new();
		private readonly Dictionary<CurveData, IPPtrCurve> m_pptrs = new();

		private readonly PathChecksumCache m_checksumCache;
		private readonly IAnimationClip m_clip;
		private readonly CustomCurveResolver m_customCurveResolver;
	}
}
