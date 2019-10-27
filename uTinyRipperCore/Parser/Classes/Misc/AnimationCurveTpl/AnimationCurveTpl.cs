using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.YAML;
using System;
using uTinyRipper.Converters.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Game.Assembly;
using uTinyRipper.Classes;

namespace uTinyRipper.Classes.Misc
{
	public struct AnimationCurveTpl<T> : IAsset, ISerializableStructure
		where T : struct, IAsset, IYAMLExportable
	{
		public AnimationCurveTpl(bool init)
		{
			PreInfinity = CurveLoopTypes.CycleWithOffset;
			PostInfinity = CurveLoopTypes.CycleWithOffset;
			RotationOrder = RotationOrder.OrderZXY;
			Curve = init ? Array.Empty<KeyframeTpl<T>>() : null;
		}

		public AnimationCurveTpl(T defaultValue, T defaultWeight) :
			this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = new KeyframeTpl<T>(0.0f, defaultValue, defaultWeight);
			Curve[1] = new KeyframeTpl<T>(1.0f, defaultValue, defaultWeight);
		}

		public AnimationCurveTpl(T value1, T value2, T defaultWeight) :
			this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = new KeyframeTpl<T>(0.0f, value1, defaultWeight);
			Curve[1] = new KeyframeTpl<T>(1.0f, value2, defaultWeight);
		}

		public AnimationCurveTpl(T value1, T inSlope1, T outSlope1, T value2, T inSlope2, T outSlope2, T defaultWeight) :
			this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = new KeyframeTpl<T>(0.0f, value1, inSlope1, outSlope1, defaultWeight);
			Curve[1] = new KeyframeTpl<T>(1.0f, value2, inSlope2, outSlope2, defaultWeight);
		}

		public AnimationCurveTpl(KeyframeTpl<T> keyframe) :
			this(false)
		{
			Curve = new KeyframeTpl<T>[1];
			Curve[0] = keyframe;
		}

		public AnimationCurveTpl(KeyframeTpl<T> keyframe1, KeyframeTpl<T> keyframe2) :
			this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = keyframe1;
			Curve[1] = keyframe2;
		}

		public AnimationCurveTpl(IReadOnlyList<KeyframeTpl<T>> keyframes) :
			this(false)
		{
			Curve = new KeyframeTpl<T>[keyframes.Count];
			for (int i = 0; i < keyframes.Count; i++)
			{
				Curve[i] = keyframes[i];
			}
		}

		public AnimationCurveTpl(IReadOnlyList<KeyframeTpl<T>> keyframes, CurveLoopTypes preInfinity, CurveLoopTypes postInfinity)
		{
			PreInfinity = preInfinity;
			PostInfinity = postInfinity;
			RotationOrder = RotationOrder.OrderZXY;
			Curve = new KeyframeTpl<T>[keyframes.Count];
			for (int i = 0; i < keyframes.Count; i++)
			{
				Curve[i] = keyframes[i];
			}
		}

		public static int ToSerializedVersion(Version version)
		{
			// unknown conversion
			if (version.IsGreaterEqual(2, 1))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRotationOrder(Version version) => version.IsGreaterEqual(5, 3);

		public ISerializableStructure CreateDuplicate()
		{
			return new AnimationCurveTpl<T>();
		}

		public AnimationCurveTpl<T> Convert(IExportContainer container)
		{
			return AnimationCurveTplConverter.Convert(container, ref this);
		}

		public void Read(AssetReader reader)
		{
			Curve = reader.ReadAssetArray<KeyframeTpl<T>>();
			reader.AlignStream(AlignType.Align4);

			PreInfinity = (CurveLoopTypes)reader.ReadInt32();
			PostInfinity = (CurveLoopTypes)reader.ReadInt32();
			if (HasRotationOrder(reader.Version))
			{
				RotationOrder = (RotationOrder)reader.ReadInt32();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.WriteAssetArray(Curve);
			writer.AlignStream(AlignType.Align4);

			writer.Write((int)PreInfinity);
			writer.Write((int)PostInfinity);
			if (HasRotationOrder(writer.Version))
			{
				writer.Write((int)RotationOrder);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(PreInfinityName, (int)PreInfinity);
			node.Add(PostInfinityName, (int)PostInfinity);
			if (HasRotationOrder(container.ExportVersion))
			{
				node.Add(RotationOrderName, (int)GetExportRotationOrder(container.Version));
			}
			return node;
		}

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield break;
		}

		private RotationOrder GetExportRotationOrder(Version version)
		{
			return HasRotationOrder(version) ? RotationOrder : RotationOrder.OrderZXY;
		}

		public KeyframeTpl<T>[] Curve { get; set; }
		public CurveLoopTypes PreInfinity { get; set; }
		public CurveLoopTypes PostInfinity { get; set; }
		public RotationOrder RotationOrder { get; set; }

		public const string CurveName = "m_Curve";
		public const string PreInfinityName = "m_PreInfinity";
		public const string PostInfinityName = "m_PostInfinity";
		public const string RotationOrderName = "m_RotationOrder";
	}
}
