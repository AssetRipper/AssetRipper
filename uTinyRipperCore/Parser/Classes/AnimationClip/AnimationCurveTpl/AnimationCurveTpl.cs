using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct AnimationCurveTpl<T> : ISerializableStructure
		where T : struct, IAssetReadable, IYAMLExportable
	{
		public AnimationCurveTpl(bool init)
		{
			PreInfinity = CurveLoopTypes.CycleWithOffset;
			PostInfinity = CurveLoopTypes.CycleWithOffset;
			RotationOrder = RotationOrder.OrderZXY;
			m_curve = init ? new KeyframeTpl<T>[0] : null;
		}

		public AnimationCurveTpl(T defaultValue, T defaultWeight) :
			this(false)
		{
			m_curve = new KeyframeTpl<T>[2];
			m_curve[0] = new KeyframeTpl<T>(0.0f, defaultValue, defaultWeight);
			m_curve[1] = new KeyframeTpl<T>(1.0f, defaultValue, defaultWeight);
		}

		public AnimationCurveTpl(T value1, T value2, T defaultWeight) :
			this(false)
		{
			m_curve = new KeyframeTpl<T>[2];
			m_curve[0] = new KeyframeTpl<T>(0.0f, value1, defaultWeight);
			m_curve[1] = new KeyframeTpl<T>(1.0f, value2, defaultWeight);
		}

		public AnimationCurveTpl(T value1, T inSlope1, T outSlope1, T value2, T inSlope2, T outSlope2, T defaultWeight) :
			this(false)
		{
			m_curve = new KeyframeTpl<T>[2];
			m_curve[0] = new KeyframeTpl<T>(0.0f, value1, inSlope1, outSlope1, defaultWeight);
			m_curve[1] = new KeyframeTpl<T>(1.0f, value2, inSlope2, outSlope2, defaultWeight);
		}

		public AnimationCurveTpl(KeyframeTpl<T> keyframe) :
			this(false)
		{
			m_curve = new KeyframeTpl<T>[1];
			m_curve[0] = keyframe;
		}

		public AnimationCurveTpl(KeyframeTpl<T> keyframe1, KeyframeTpl<T> keyframe2) :
			this(false)
		{
			m_curve = new KeyframeTpl<T>[2];
			m_curve[0] = keyframe1;
			m_curve[1] = keyframe2;
		}

		public AnimationCurveTpl(IReadOnlyList<KeyframeTpl<T>> keyframes) :
			this(false)
		{
			m_curve = new KeyframeTpl<T>[keyframes.Count];
			for (int i = 0; i < keyframes.Count; i++)
			{
				m_curve[i] = keyframes[i];
			}
		}

		public AnimationCurveTpl(IReadOnlyList<KeyframeTpl<T>> keyframes, CurveLoopTypes preInfinity, CurveLoopTypes postInfinity)
		{
			PreInfinity = preInfinity;
			PostInfinity = postInfinity;
			RotationOrder = RotationOrder.OrderZXY;
			m_curve = new KeyframeTpl<T>[keyframes.Count];
			for (int i = 0; i < keyframes.Count; i++)
			{
				m_curve[i] = keyframes[i];
			}
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadRotationOrder(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2, 1))
			{
				return 2;
			}
			return 1;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new AnimationCurveTpl<T>();
		}

		public void Read(AssetReader reader)
		{
			m_curve = reader.ReadAssetArray<KeyframeTpl<T>>();
			reader.AlignStream(AlignType.Align4);

			PreInfinity = (CurveLoopTypes)reader.ReadInt32();
			PostInfinity = (CurveLoopTypes)reader.ReadInt32();
			if (IsReadRotationOrder(reader.Version))
			{
				RotationOrder = (RotationOrder)reader.ReadInt32();
			}
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("m_Curve", Curve.ExportYAML(container));
			node.Add("m_PreInfinity", (int)PreInfinity);
			node.Add("m_PostInfinity", (int)PostInfinity);
			node.Add("m_RotationOrder", (int)GetExportRotationOrder(container.Version));

			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		private RotationOrder GetExportRotationOrder(Version version)
		{
			return IsReadRotationOrder(version) ? RotationOrder : RotationOrder.OrderZXY;
		}

		public IReadOnlyList<KeyframeTpl<T>> Curve => m_curve;
		public CurveLoopTypes PreInfinity { get; private set; }
		public CurveLoopTypes PostInfinity { get; private set; }
		public RotationOrder RotationOrder { get; private set; }

		private KeyframeTpl<T>[] m_curve;
	}
}
