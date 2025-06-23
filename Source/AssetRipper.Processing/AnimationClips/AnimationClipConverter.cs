using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Checksum;
using AssetRipper.IO.Endian;
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
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace AssetRipper.Processing.AnimationClips;

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
		AnimationClipConverter converter = new(clip, checksumCache);
		converter.ProcessInner();
	}

	private void ProcessInner()
	{
		if (m_clip.Has_ClipBindingConstant_C74())
		{
			IClip clip = m_clip.MuscleClip_C74.Clip.Data;

			IReadOnlyList<StreamedFrame> streamedFrames = GenerateFramesFromStreamedClip(clip.StreamedClip);
			ProcessStreams(streamedFrames);

			int streamedCurveCount = clip.StreamedClip.CurveCount();
			ProcessDenses(clip.DenseClip, streamedCurveCount);

			if (clip.Has_ConstantClip())
			{
				int preConstantCurves = streamedCurveCount + (int)clip.DenseClip.CurveCount;
				float lastConstantTime = CalculateLastConstantTime(streamedFrames, m_clip.MuscleClip_C74.StopTime);
				ProcessConstant(clip.ConstantClip, preConstantCurves, lastConstantTime);
			}

			m_clip.MuscleClipInfo_C74.Initialize(m_clip.MuscleClip_C74);
		}
	}

	private void ProcessStreams(IReadOnlyList<StreamedFrame> streamedFrames)
	{
		Span<float> curveValues = [0, 0, 0, 0];
		Span<float> inSlopeValues = [0, 0, 0, 0];
		Span<float> outSlopeValues = [0, 0, 0, 0];
		bool UseNegInfSlopes = m_clip.SupportsNegativeInfinitySlopes();

		if (streamedFrames.Count > 1)
		{
			streamedFrames[0].Time = 0f; // fix first frame for PPtrCurves to have Time=0 instead of float.MinValue
		}
		int frameCount = streamedFrames.Count - 1; // last StreamedFrame is dummy so must be skipped
		for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
		{
			// last real frame doesn't need outSlope calculation, and will have inSlope from previous iteration
			bool doSlopeCalc = frameIdx != frameCount - 1;

			StreamedFrame frame = streamedFrames[frameIdx];
			for (int curveIdx = 0; curveIdx < frame.Curves.Length;)
			{
				int curveID = frame.Curves[curveIdx].Index;
				IGenericBinding binding = GetBinding(curveID);
				string path = GetCurvePath(binding.Path);
				StreamedCurveKey curve;
				if (binding.IsTransform())
				{
					int transformDim = binding.TransformType().GetDimension();
					if (frameIdx == 0) // first StreamedFrame only contains PPtrCurves, skip
					{
						curveIdx += transformDim;
						continue;
					}
					for (int offset = 0; offset < transformDim; offset++)
					{
						curve = frame.Curves[curveIdx];
						if (doSlopeCalc)
						{
							if (TryGetNextFrame(streamedFrames, frameIdx, curveID, out StreamedFrame? nextFrame, out int nextCurveIdx))
							{
								StreamedCurveKey nextCurve = nextFrame.Curves[nextCurveIdx + offset];
								curve.CalculateSlopes(frame.Time, nextFrame.Time, nextCurve, UseNegInfSlopes);
							}
						}
						curveValues[offset] = curve.Value;
						inSlopeValues[offset] = curve.InSlope;
						outSlopeValues[offset] = curve.OutSlope;
						curveIdx++;
					}
					AddTransformCurve(frame.Time, binding, curveValues, inSlopeValues, outSlopeValues, 0, path);
					continue;
				}
				curve = frame.Curves[curveIdx];
				if (!binding.IsPPtrCurve()) // Skip slope calculation for PPtrCurves
				{
					if (frameIdx == 0) // first StreamedFrame only contains PPtrCurves, skip
					{
						curveIdx++;
						continue;
					}
					if (doSlopeCalc)
					{
						if (TryGetNextFrame(streamedFrames, frameIdx, curveID, out StreamedFrame? nextFrame, out int nextCurveIdx))
						{
							StreamedCurveKey nextCurve = nextFrame.Curves[nextCurveIdx];
							curve.CalculateSlopes(frame.Time, nextFrame.Time, nextCurve, UseNegInfSlopes);
						}
					}
				}
				if (binding.CustomType == (byte)BindingCustomType.None)
				{
					AddDefaultCurve(binding, path, frame.Time, curve.Value, curve.InSlope, curve.OutSlope);
				}
				else
				{
					AddCustomCurve(binding, path, frame.Time, curve.Value, curve.InSlope, curve.OutSlope);
				}
				curveIdx++;
			}
		}
	}

	private void ProcessDenses(DenseClip dense, int preDenseCurves)
	{
		ReadOnlySpan<float> slopeValues = [0, 0, 0, 0]; // no slopes - 0 values

		float[] rentedArray = ArrayPool<float>.Shared.Rent(dense.SampleArray.Count);
		dense.SampleArray.CopyTo(rentedArray);
		ReadOnlySpan<float> curveValues = new(rentedArray, 0, dense.SampleArray.Count);

		for (int frameIndex = 0; frameIndex < dense.FrameCount; frameIndex++)
		{
			float time = frameIndex / dense.SampleRate + dense.BeginTime;
			int frameOffset = frameIndex * (int)dense.CurveCount;
			for (int curveIndex = 0; curveIndex < dense.CurveCount;)
			{
				int index = preDenseCurves + curveIndex;
				IGenericBinding binding = GetBinding(index);
				string path = GetCurvePath(binding.Path);
				int framePosition = frameOffset + curveIndex;
				if (binding.IsTransform())
				{
					AddTransformCurve(time, binding, curveValues, slopeValues, slopeValues, framePosition, path);
					curveIndex += binding.TransformType().GetDimension();
				}
				else if (binding.CustomType == (byte)BindingCustomType.None)
				{
					AddDefaultCurve(binding, path, time, dense.SampleArray[framePosition]);
					curveIndex++;
				}
				else
				{
					AddCustomCurve(binding, path, time, dense.SampleArray[framePosition]);
					curveIndex++;
				}
			}
		}
		ArrayPool<float>.Shared.Return(rentedArray);
	}

	private void ProcessConstant(IConstantClip constant, int preConstantCurves, float lastFrame)
	{
		float[] rentedArray = ArrayPool<float>.Shared.Rent(constant.Data.Count);
		constant.Data.CopyTo(rentedArray);
		ReadOnlySpan<float> curveValues = new(rentedArray, 0, constant.Data.Count);

		ReadOnlySpan<float> slopeValues = [0, 0, 0, 0]; // no slopes - 0 values

		float time = 0f;
		int Is1or2Frames = time == lastFrame ? 1 : 2; // a constant curve can be made with 1 or 2 frames
		for (int i = 0; i < Is1or2Frames; i++, time += lastFrame)
		{
			for (int curveIndex = 0; curveIndex < constant.Data.Count;)
			{
				int index = preConstantCurves + curveIndex;
				IGenericBinding binding = GetBinding(index);
				string path = GetCurvePath(binding.Path);
				if (binding.IsTransform())
				{
					AddTransformCurve(time, binding, curveValues, slopeValues, slopeValues, curveIndex, path);
					curveIndex += binding.TransformType().GetDimension();
				}
				else if (binding.CustomType == (byte)BindingCustomType.None)
				{
					AddDefaultCurve(binding, path, time, constant.Data[curveIndex]);
					curveIndex++;
				}
				else
				{
					AddCustomCurve(binding, path, time, constant.Data[curveIndex]);
					curveIndex++;
				}
			}
		}
		ArrayPool<float>.Shared.Return(rentedArray);
	}

	private void AddCustomCurve(IGenericBinding binding, string path, float time, float value, float inTangent = 0, float outTangent = 0)
	{
		switch ((BindingCustomType)binding.CustomType)
		{
			case BindingCustomType.AnimatorMuscle:
				AddAnimatorMuscleCurve(binding, time, value, inTangent, outTangent);
				break;

			default:
				string attribute = m_customCurveResolver.ToAttributeName((BindingCustomType)binding.CustomType, binding.Attribute, path);
				CurveData curve = new(path, attribute, binding.GetClassID(), binding.Script.TryGetAsset(m_clip.Collection));
				if (binding.IsPPtrCurve())
				{
					AddPPtrKeyframe(curve, time, (int)value);
				}
				else
				{
					AddFloatKeyframe(curve, time, value, inTangent, outTangent);
				}
				break;
		}
	}

	private void AddTransformCurve(float time, IGenericBinding binding, ReadOnlySpan<float> curveValues,
		ReadOnlySpan<float> inSlopeValues, ReadOnlySpan<float> outSlopeValues, int offset, string path)
	{
		switch (binding.TransformType())
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
						curve.SetValues(path, (RotationOrder)binding.CustomType);
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
				throw new NotImplementedException(binding.TransformType().ToString());
		}
	}

	private void AddDefaultCurve(IGenericBinding binding, string path, float time, float value, float inTangent = 0, float outTangent = 0)
	{
		switch (binding.GetClassID())
		{
			case ClassIDType.GameObject:
				AddGameObjectCurve(binding, path, time, value, inTangent, outTangent);
				break;

			case ClassIDType.MonoBehaviour:
				AddScriptCurve(binding, path, time, value, inTangent, outTangent);
				break;

			default:
				AddEngineCurve(binding, path, time, value, inTangent, outTangent);
				break;
		}
	}

	private void AddGameObjectCurve(IGenericBinding binding, string path, float time, float value, float inTangent, float outTangent)
	{
		if (GameObject.TryGetPath(binding.Attribute, out string? propertyName))
		{
			CurveData curve = new(path, propertyName, ClassIDType.GameObject);
			AddFloatKeyframe(curve, time, value, inTangent, outTangent);
		}
		else
		{
			// that means that dev exported animation clip with missing component
			CurveData curve = new(path, GetReversedPath(MissedPropertyPrefix, binding.Attribute), ClassIDType.GameObject);
			AddFloatKeyframe(curve, time, value, inTangent, outTangent);
		}
	}

	private void AddScriptCurve(IGenericBinding binding, string path, float time, float value, float inTangent, float outTangent)
	{
		if (binding.Script.TryGetAsset(m_clip.Collection) is IMonoScript script)
		{
			m_checksumCache.Add(script);
		}

		if (!m_checksumCache.TryGetPath(binding.Attribute, out string? propertyName))
		{
			propertyName = GetReversedPath(ScriptPropertyPrefix, binding.Attribute);
		}

		CurveData curve = new(path, propertyName, ClassIDType.MonoBehaviour, binding.Script.TryGetAsset(m_clip.Collection));

		if (binding.IsPPtrCurve())
		{
			AddPPtrKeyframe(curve, time, (int)value);
		}
		else
		{
			AddFloatKeyframe(curve, time, value, inTangent, outTangent);
		}
	}

	private void AddEngineCurve(IGenericBinding binding, string path, float time, float value, float inTangent, float outTangent)
	{
		if (!FieldHashes.TryGetPath(binding.GetClassID(), binding.Attribute, out string? propertyName))
		{
			propertyName = GetReversedPath(TypeTreePropertyPrefix, binding.Attribute);
		}

		CurveData curve = new(path, propertyName, binding.GetClassID());

		if (binding.IsPPtrCurve())
		{
			AddPPtrKeyframe(curve, time, (int)value);
		}
		else
		{
			AddFloatKeyframe(curve, time, value, inTangent, outTangent);
		}
	}

	private void AddAnimatorMuscleCurve(IGenericBinding binding, float time, float value, float inTangent, float outTangent)
	{
		string attributeString = HumanoidMuscleTypeExtensions.ToAttributeString(binding.GetHumanoidMuscle(Version));
		CurveData curve = new(string.Empty, attributeString, ClassIDType.Animator);
		AddFloatKeyframe(curve, time, value, inTangent, outTangent);
	}

	private void AddFloatKeyframe(in CurveData curveData, float time, float value, float inTangent, float outTangent)
	{
		if (!m_floats.TryGetValue(curveData, out IFloatCurve? curve))
		{
			curve = m_clip.FloatCurves_C74.AddNew();
			curve.Path = curveData.Path;
			curve.Attribute = curveData.Attribute;
			curve.ClassID = (int)curveData.ClassID;
			curve.Script.SetAsset(m_clip.Collection, curveData.Script as IMonoScript);
			curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
			//Todo: set IFloatCurve.Flags or verify that 0 is an acceptable value.
			m_floats.Add(curveData, curve);
		}

		IKeyframe_Single floatKey = curve.Curve.Curve.AddNew();
		floatKey.SetValues(Version, time, value, inTangent, outTangent, DefaultFloatWeight);
	}

	private void AddPPtrKeyframe(in CurveData curveData, float time, int index)
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
			//Not certain this enum is correct, but it seems to be. 2 is the correct value for this field.
			//See: https://github.com/AssetRipper/AssetRipper/issues/1158
			curve.Flags = (int)SourceGenerated.NativeEnums.Global.EditorCurveBindingFlags.PPtr;
			m_pptrs.Add(curveData, curve);
		}

		IPPtr_Object value = ClipBindingConstant.PptrCurveMapping[index];
		IPPtrKeyframe key = curve.Curve.AddNew();
		key.Time = time;
		key.Value.CopyValues(value, new PPtrConverter(m_clip));
	}

	private IGenericBinding GetBinding(int index)
	{
		if (m_bindingsCache.TryGetValue(index, out IGenericBinding? binding))
		{
			return binding;
		}
		int curves = 0;
		AccessListBase<IGenericBinding> bindings = ClipBindingConstant.GenericBindings;
		for (int i = 0; i < bindings.Count; i++)
		{
			binding = bindings[i];
			if (binding.GetClassID() == ClassIDType.Transform)
			{
				curves += binding.TransformType().GetDimension();
			}
			else
			{
				curves += 1;
			}
			if (curves > index)
			{
				m_bindingsCache[index] = binding;
				if (binding.IsTransform() && binding.TransformType().GetDimension() < 1)
				{
					// If an animation was malformed, this avoids the possibility of an infinite FOR loop when processing Transform bindings
					throw new IndexOutOfRangeException("Transform AnimationCurve can't have Dimension less than 1.");
				}
				return binding;
			}
		}
		throw new ArgumentException($"Binding with index {index} hasn't been found", nameof(index));
	}

	private static bool TryGetNextFrame(IReadOnlyList<StreamedFrame> streamedFrames, int currentFrame, int curveID, [MaybeNullWhen(false)] out StreamedFrame nextFrame, out int curveIdx)
	{
		for (int frameIndex = currentFrame + 1; frameIndex < streamedFrames.Count; frameIndex++)
		{
			nextFrame = streamedFrames[frameIndex];
			for (curveIdx = 0; curveIdx < nextFrame.Curves.Length; curveIdx++)
			{
				StreamedCurveKey curve = nextFrame.Curves[curveIdx];
				if (curve.Index == curveID)
				{
					return true;
				}
			}
		}
		nextFrame = null;
		curveIdx = -1;
		return false;
	}

	private string GetCurvePath(uint hash)
	{
		if (m_checksumCache.TryGetPath(hash, out string? path))
		{
			return path;
		}
		else
		{
			return GetReversedPath(UnknownPathPrefix, hash);
		}
	}

	public IReadOnlyList<StreamedFrame> GenerateFramesFromStreamedClip(IStreamedClip clip)
	{
		List<StreamedFrame> frames = new();
		Span<byte> buffer = new byte[clip.Data.Count * sizeof(uint)];
		AssetCollection collection = m_clip.Collection;
		CopyDataToBuffer(clip, collection, buffer);

		EndianSpanReader reader = new(buffer, collection.EndianType);
		while (reader.Position < reader.Length)
		{
			StreamedFrame frame = new();
			frame.Read(ref reader, collection.Version);
			frames.Add(frame);
		}
		return frames;

		static bool CpuEndiannessMatchesCollection(AssetCollection collection)
		{
			return (BitConverter.IsLittleEndian && collection.EndianType is EndianType.LittleEndian)
				|| (!BitConverter.IsLittleEndian && collection.EndianType is EndianType.BigEndian);
		}

		static void CopyDataToBuffer(IStreamedClip clip, AssetCollection collection, Span<byte> buffer)
		{
			if (CpuEndiannessMatchesCollection(collection))
			{
				Span<uint> span = MemoryMarshal.Cast<byte, uint>(buffer);
				clip.Data.CopyTo(span);
			}
			else
			{
				for (int i = 0; i < clip.Data.Count; i++)
				{
					if (BitConverter.IsLittleEndian)
					{
						BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(i * sizeof(uint)), clip.Data[i]);
					}
					else
					{
						BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(i * sizeof(uint)), clip.Data[i]);
					}
				}
			}
		}
	}

	private float CalculateLastConstantTime(IReadOnlyList<StreamedFrame> streamedFrames, float stopTime)
	{
		if (stopTime == 0f || streamedFrames.Count <= 1) // streamedFrames[streamedFrames.Count-1] has dummy Infinity Time
		{
			return stopTime;
		}
		float sampleRate = m_clip.SampleRate_C74;
		// using frame indexes for precise calculation
		int lastFrame = (int)float.Round(stopTime * sampleRate);
		StreamedFrame lastStreamedFrame = streamedFrames[streamedFrames.Count - 2];
		// careful of streamedFrames[0], has Time=float.MinValue
		int lastSFFrame = lastStreamedFrame.Time > 0 ? (int)float.Round(lastStreamedFrame.Time * sampleRate) : 0;
		if (lastFrame - lastSFFrame == 1)
		{
			// check if last StreamedFrame has a PPtrCurve, because it adds 1 extra frame to MuscleClip.StopTime
			foreach (StreamedCurveKey curve in lastStreamedFrame.Curves)
			{
				IGenericBinding binding = GetBinding(curve.Index);
				if (binding.IsPPtrCurve())
				{
					// careful of streamedFrames[0], has Time=float.MinValue
					return lastStreamedFrame.Time > 0 ? lastStreamedFrame.Time : 0f;
				}
			}
		}
		return stopTime;
	}

	private static string GetReversedPath([ConstantExpected] string prefix, uint hash)
	{
		return Crc32Algorithm.ReverseAscii(hash, $"{prefix}0x{hash:X}_");
	}

	private UnityVersion Version => m_clip.Collection.Version;

	private const string UnknownPathPrefix = "path_";
	private const string MissedPropertyPrefix = "missed_";
	private const string ScriptPropertyPrefix = "script_";
	private const string TypeTreePropertyPrefix = "typetree_";

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
	private readonly Dictionary<int, IGenericBinding> m_bindingsCache = new(); //cache results from GetBinding(curveID)
	private IAnimationClipBindingConstant ClipBindingConstant => m_clip.ClipBindingConstant_C74!; //This class only supports 4.3 and newer.
}
