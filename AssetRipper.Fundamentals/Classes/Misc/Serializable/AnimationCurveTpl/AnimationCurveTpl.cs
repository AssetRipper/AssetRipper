using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Equality;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl
{
	public sealed class AnimationCurveTpl<T> : IAsset, IEquatable<AnimationCurveTpl<T>> where T : IAsset, IYamlExportable, new()
	{
		public AnimationCurveTpl() { }

		public AnimationCurveTpl(bool _)
		{
			PreInfinity = CurveLoopTypes.CycleWithOffset;
			PostInfinity = CurveLoopTypes.CycleWithOffset;
			RotationOrder = RotationOrder.OrderZXY;
		}

		public AnimationCurveTpl(T defaultValue, T defaultWeight) : this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = new KeyframeTpl<T>(0.0f, defaultValue, defaultWeight);
			Curve[1] = new KeyframeTpl<T>(1.0f, defaultValue, defaultWeight);
		}

		public AnimationCurveTpl(T value1, T value2, T defaultWeight) : this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = new KeyframeTpl<T>(0.0f, value1, defaultWeight);
			Curve[1] = new KeyframeTpl<T>(1.0f, value2, defaultWeight);
		}

		public AnimationCurveTpl(T value1, T inSlope1, T outSlope1, T value2, T inSlope2, T outSlope2, T defaultWeight) : this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = new KeyframeTpl<T>(0.0f, value1, inSlope1, outSlope1, defaultWeight);
			Curve[1] = new KeyframeTpl<T>(1.0f, value2, inSlope2, outSlope2, defaultWeight);
		}

		public AnimationCurveTpl(KeyframeTpl<T> keyframe) : this(false)
		{
			Curve = new KeyframeTpl<T>[1];
			Curve[0] = keyframe;
		}

		public AnimationCurveTpl(KeyframeTpl<T> keyframe1, KeyframeTpl<T> keyframe2) : this(false)
		{
			Curve = new KeyframeTpl<T>[2];
			Curve[0] = keyframe1;
			Curve[1] = keyframe2;
		}

		public AnimationCurveTpl(IReadOnlyList<KeyframeTpl<T>> keyframes) : this(false)
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

		public void Read(AssetReader reader)
		{
			Curve = reader.ReadAssetArray<KeyframeTpl<T>>();
			reader.AlignStream();

			PreInfinity = (CurveLoopTypes)reader.ReadInt32();
			PostInfinity = (CurveLoopTypes)reader.ReadInt32();
			if (HasRotationOrder(reader.Version))
			{
				RotationOrder = (RotationOrder)reader.ReadInt32();
			}
		}

		public void Write(AssetWriter writer)
		{
			Curve.Write(writer);
			writer.AlignStream();

			writer.Write((int)PreInfinity);
			writer.Write((int)PostInfinity);
			if (HasRotationOrder(writer.Version))
			{
				writer.Write((int)RotationOrder);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(CurveName, Curve.ExportYaml(container));
			node.Add(PreInfinityName, (int)PreInfinity);
			node.Add(PostInfinityName, (int)PostInfinity);
			if (HasRotationOrder(container.ExportVersion))
			{
				node.Add(RotationOrderName, (int)GetExportRotationOrder(container.Version));
			}
			return node;
		}

		private RotationOrder GetExportRotationOrder(UnityVersion version)
		{
			return HasRotationOrder(version) ? RotationOrder : RotationOrder.OrderZXY;
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2, 1))
			{
				// unknown conversion
				return 2;
			}
			else
			{
				return 1;
			}
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRotationOrder(UnityVersion version) => version.IsGreaterEqual(5, 3);

		public override bool Equals(object? obj)
		{
			if (obj is AnimationCurveTpl<T> curve)
			{
				return Equals(curve);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public bool Equals(AnimationCurveTpl<T>? other)
		{
			return
				other is not null &&
				PreInfinity == other.PreInfinity &&
				PostInfinity == other.PostInfinity &&
				RotationOrder == other.RotationOrder &&
				ArrayEquality.AreEqualArrays(Curve, other.Curve);
		}

		public KeyframeTpl<T>[] Curve { get; set; } = Array.Empty<KeyframeTpl<T>>();
		public CurveLoopTypes PreInfinity { get; set; }
		public CurveLoopTypes PostInfinity { get; set; }
		public RotationOrder RotationOrder { get; set; }

		public const string CurveName = "m_Curve";
		public const string PreInfinityName = "m_PreInfinity";
		public const string PostInfinityName = "m_PostInfinity";
		public const string RotationOrderName = "m_RotationOrder";
	}
}
