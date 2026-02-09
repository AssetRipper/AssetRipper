using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace AssetRipper.Numerics;

public record struct BoneWeight4
{
	public const int Count = 4;
	public WeightArray Weights;
	public IndexArray Indices;

	public BoneWeight4(float weight0, float weight1, float weight2, float weight3, int index0, int index1, int index2, int index3)
		: this(new WeightArray(weight0, weight1, weight2, weight3), new IndexArray(index0, index1, index2, index3))
	{
	}

	public BoneWeight4(WeightArray weights, IndexArray indices)
	{
		Weights = weights;
		Indices = indices;
	}

	public float Weight0
	{
		readonly get => Weights[0];
		set => Weights[0] = value;
	}

	public float Weight1
	{
		readonly get => Weights[1];
		set => Weights[1] = value;
	}

	public float Weight2
	{
		readonly get => Weights[2];
		set => Weights[2] = value;
	}

	public float Weight3
	{
		readonly get => Weights[3];
		set => Weights[3] = value;
	}

	public int Index0
	{
		readonly get => Indices[0];
		set => Indices[0] = value;
	}

	public int Index1
	{
		readonly get => Indices[1];
		set => Indices[1] = value;
	}

	public int Index2
	{
		readonly get => Indices[2];
		set => Indices[2] = value;
	}

	public int Index3
	{
		readonly get => Indices[3];
		set => Indices[3] = value;
	}

	public readonly bool AnyWeightsNegative => Weight0 < 0f || Weight1 < 0f || Weight2 < 0f || Weight3 < 0f;

	public readonly float Sum => Weight0 + Weight1 + Weight2 + Weight3;

	public readonly bool Normalized => Sum == 1f;

	[Pure]
	public readonly BoneWeight4 NormalizeWeights()
	{
		float sum = Sum;
		if (sum == 0f)
		{
			return new BoneWeight4(.25f, .25f, .25f, .25f, Index0, Index1, Index2, Index3);
		}
		else
		{
			float invSum = 1f / sum;
			return new BoneWeight4(Weight0 * invSum, Weight1 * invSum, Weight2 * invSum, Weight3 * invSum, Index0, Index1, Index2, Index3);
		}
	}

	public override readonly string ToString()
	{
		return $"{nameof(BoneWeight4)}: {{ {nameof(Weights)} = {Weights}, {nameof(Indices)} = {Indices} }}";
	}

	[InlineArray(Count)]
	public struct WeightArray : IEquatable<WeightArray>
	{
		private float _element0;

		public WeightArray(float weight0, float weight1, float weight2, float weight3)
		{
			this[0] = weight0;
			this[1] = weight1;
			this[2] = weight2;
			this[3] = weight3;
		}

		public override readonly bool Equals(object? obj)
		{
			return obj is WeightArray array && Equals(array);
		}

		public readonly bool Equals(WeightArray other)
		{
			return ((ReadOnlySpan<float>)this).SequenceEqual(other);
		}

		public override readonly int GetHashCode()
		{
			return HashCode.Combine(this[0], this[1], this[2], this[3]);
		}

		public static bool operator ==(WeightArray left, WeightArray right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(WeightArray left, WeightArray right)
		{
			return !(left == right);
		}

		public readonly void Deconstruct(out float weight0, out float weight1, out float weight2, out float weight3)
		{
			weight0 = this[0];
			weight1 = this[1];
			weight2 = this[2];
			weight3 = this[3];
		}

		public override readonly string ToString()
		{
			return $"[{this[0]}, {this[1]}, {this[2]}, {this[3]}]";
		}
	}

	[InlineArray(Count)]
	public struct IndexArray : IEquatable<IndexArray>
	{
		private int _element0;

		public IndexArray(int index0, int index1, int index2, int index3)
		{
			this[0] = index0;
			this[1] = index1;
			this[2] = index2;
			this[3] = index3;
		}

		public override readonly bool Equals(object? obj)
		{
			return obj is IndexArray array && Equals(array);
		}

		public readonly bool Equals(IndexArray other)
		{
			return ((ReadOnlySpan<int>)this).SequenceEqual(other);
		}

		public override readonly int GetHashCode()
		{
			return HashCode.Combine(this[0], this[1], this[2], this[3]);
		}

		public static bool operator ==(IndexArray left, IndexArray right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(IndexArray left, IndexArray right)
		{
			return !(left == right);
		}

		public readonly void Deconstruct(out int index0, out int index1, out int index2, out int index3)
		{
			index0 = this[0];
			index1 = this[1];
			index2 = this[2];
			index3 = this[3];
		}

		public override readonly string ToString()
		{
			return $"[{this[0]}, {this[1]}, {this[2]}, {this[3]}]";
		}
	}
}
