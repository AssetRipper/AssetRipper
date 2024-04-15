using System.Collections;
using System.Text;

namespace AssetRipper.Numerics;

/// <summary>
/// An immutable structure representing a discontinuous, possibly empty, range of objects.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct DiscontinuousRange<T> : IEquatable<DiscontinuousRange<T>>, IEquatable<Range<T>>, IReadOnlyList<Range<T>>
	where T : notnull, IComparable<T>, IEquatable<T>
{
	private static readonly List<Range<T>> defaultRangeList = new List<Range<T>>(0);
	private readonly List<Range<T>> rangeList;

	public static DiscontinuousRange<T> Empty { get; } = new();

	public DiscontinuousRange()
	{
		rangeList = defaultRangeList;
	}

	public DiscontinuousRange(Range<T> range)
	{
		rangeList = new List<Range<T>>(1) { range };
	}

	public DiscontinuousRange(IEnumerable<Range<T>> ranges)
	{
		rangeList = new();
		Add(ranges);
		if (rangeList.Count == 0)
		{
			rangeList = defaultRangeList;
		}
	}

	public DiscontinuousRange(IReadOnlyList<Range<T>> ranges)
	{
		if (ranges.Count == 0)
		{
			rangeList = defaultRangeList;
		}
		else
		{
			rangeList = new List<Range<T>>(ranges.Count);
			Add(ranges);
		}
	}

	public DiscontinuousRange(params Range<T>[] ranges)
	{
		rangeList = new List<Range<T>>(ranges.Length);
		Add(ranges);
	}

	/// <summary>
	/// Used only for internal creations of the range list.
	/// </summary>
	/// <param name="ranges"></param>
	private DiscontinuousRange(List<Range<T>> rangeList)
	{
		this.rangeList = rangeList;
	}

	/// <summary>
	/// Constructor for the union of two ranges.
	/// </summary>
	/// <param name="range1"></param>
	/// <param name="range2"></param>
	public DiscontinuousRange(DiscontinuousRange<T> range1, DiscontinuousRange<T> range2)
	{
		if (range1.Count == 0)
		{
			rangeList = range2.rangeList;//prevent unnecessary allocation of an additional list
		}
		else if (range2.Count == 0)
		{
			rangeList = range1.rangeList;//prevent unnecessary allocation of an additional list
		}
		else
		{
			rangeList = new List<Range<T>>(range1.Count);
			rangeList.AddRange(range1.rangeList);//Copy the elements of range1
			Add(range2.rangeList);//Add the elements of range2
		}
	}

	public int Count => rangeList.Count;

	public Range<T> this[int index] => rangeList[index];

	public override bool Equals(object? obj)
	{
		return obj is Range<T> range && Equals(range)
			|| obj is DiscontinuousRange<T> discontinuousRange && Equals(discontinuousRange);
	}

	public bool Equals(Range<T> other)
	{
		return Count == 1 && this[0] == other;
	}

	public bool Equals(DiscontinuousRange<T> other)
	{
		if (Count != other.Count)
		{
			return false;
		}

		for (int i = 0; i < rangeList.Count; i++)
		{
			if (!this[i].Equals(other[i]))
			{
				return false;
			}
		}

		return true;
	}

	public bool Contains(T point)
	{
		return rangeList.Any(r => r.Contains(point));
	}

	public bool Contains(Range<T> range)
	{
		return rangeList.Any(r => r.Contains(range));
	}

	public bool Contains(DiscontinuousRange<T> other)
	{
		foreach (Range<T> range in other.rangeList)
		{
			if (!Contains(range))
			{
				return false;
			}
		}
		return true;
	}

	public bool Intersects(Range<T> range)
	{
		return rangeList.Any(r => r.Intersects(range));
	}

	public bool Intersects(DiscontinuousRange<T> other)
	{
		int i = 0;
		int j = 0;
		while (i < Count && j < other.Count)
		{
			Range<T> thisRange = this[i];
			Range<T> otherRange = other[j];
			if (thisRange.Intersects(otherRange))
			{
				return true;
			}
			else if (thisRange.IsStrictlyLess(otherRange))
			{
				i++;
			}
			else//otherRange is strictly less than thisRange
			{
				j++;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		HashCode hc = new();
		for (int i = Count - 1; i >= 0; i--)
		{
			hc.Add(this[i]);
		}
		return hc.ToHashCode();
	}

	/// <summary>
	/// Only used during initialization
	/// </summary>
	/// <param name="range"></param>
	private void Add(Range<T> range)
	{
		int firstUnionIndex = Count;
		int lastUnionIndex = Count;
		for (int i = 0; i < Count; i++)
		{
			if (this[i].CanUnion(range))
			{
				if (firstUnionIndex == Count)
				{
					firstUnionIndex = i;
				}
				lastUnionIndex = i;
			}
			else if (firstUnionIndex < Count)
			{
				break;
			}
			else if (rangeList[i].IsStrictlyGreater(range))
			{
				rangeList.Insert(i, range);
				return;
			}
		}

		if (firstUnionIndex == Count)
		{
			rangeList.Add(range);
		}
		else
		{
			rangeList[firstUnionIndex] = rangeList[firstUnionIndex].MakeUnion(range);
			if (firstUnionIndex < lastUnionIndex)
			{
				rangeList.RemoveRange(firstUnionIndex + 1, lastUnionIndex - firstUnionIndex);
			}
		}
	}

	/// <summary>
	/// Only used during initialization
	/// </summary>
	/// <param name="ranges"></param>
	private void Add(IEnumerable<Range<T>> ranges)
	{
		foreach (Range<T> range in ranges)
		{
			Add(range);
		}
	}

	/// <summary>
	/// Only used during initialization
	/// </summary>
	/// <param name="ranges"></param>
	private void Add(Range<T>[] ranges)
	{
		for (int i = 0; i < ranges.Length; i++)
		{
			Add(ranges[i]);
		}
	}

	/// <summary>
	/// This range contains no points and is equal to the <see cref="Empty"/> range.
	/// </summary>
	/// <returns></returns>
	public bool IsEmpty() => Count == 0;

	/// <summary>
	/// This range is continuous.
	/// </summary>
	/// <remarks>
	/// The <see cref="Empty"/> range is treated as not continuous.
	/// </remarks>
	/// <returns></returns>
	public bool IsContinuous() => Count == 1;

	/// <summary>
	/// This range is continuous.
	/// </summary>
	/// <remarks>
	/// The <see cref="Empty"/> range is treated as not continuous.
	/// </remarks>
	/// <param name="range"></param>
	/// <returns></returns>
	public bool IsContinuous(out Range<T> range)
	{
		if (Count == 1)
		{
			range = this[0];
			return true;
		}
		else
		{
			range = default;
			return false;
		}
	}

	public DiscontinuousRange<T> Negate(T minimum, T maximum)
	{
		if (IsEmpty())
		{
			return new DiscontinuousRange<T>(new Range<T>(minimum, maximum));
		}
		else
		{
			List<Range<T>> newRangeList = new List<Range<T>>(Count + 1);//Count + 1 is the maximum possible size of this list.

			T start = rangeList[0].Start;
			if (!start.Equals(minimum))
			{
				newRangeList.Add(new Range<T>(minimum, start));
			}

			for (int i = 0; i < Count - 1; i++)
			{
				newRangeList.Add(new Range<T>(rangeList[i].End, rangeList[i].Start));
			}

			T end = rangeList[Count - 1].End;
			if (!end.Equals(maximum))
			{
				newRangeList.Add(new Range<T>(end, maximum));
			}

			return new DiscontinuousRange<T>(newRangeList);
		}
	}

	public IEnumerator<Range<T>> GetEnumerator() => rangeList.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public static bool operator ==(DiscontinuousRange<T> left, DiscontinuousRange<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(DiscontinuousRange<T> left, DiscontinuousRange<T> right)
	{
		return !(left == right);
	}

	public static implicit operator DiscontinuousRange<T>(Range<T> range) => new DiscontinuousRange<T>(range);

	public static explicit operator Range<T>(DiscontinuousRange<T> range)
	{
		return range.IsContinuous(out Range<T> continuousRange)
			? continuousRange
			: throw new ArgumentException($"{nameof(range)} is not continuous.", nameof(range));
	}

	public static DiscontinuousRange<T> Union(DiscontinuousRange<T> range1, DiscontinuousRange<T> range2)
	{
		return new DiscontinuousRange<T>(range1, range2);
	}

	public DiscontinuousRange<T> Union(DiscontinuousRange<T> other)
	{
		return new DiscontinuousRange<T>(this, other);
	}

	public static DiscontinuousRange<T> Intersect(DiscontinuousRange<T> range1, DiscontinuousRange<T> range2)
	{
		return range1.Intersect(range2);
	}

	public DiscontinuousRange<T> Intersect(DiscontinuousRange<T> other)
	{
		List<Range<T>> ranges = new();
		int i = 0;
		int j = 0;
		while (i < Count && j < other.Count)
		{
			Range<T> thisRange = this[i];
			Range<T> otherRange = other[j];
			if (thisRange.Intersects(otherRange, out Range<T> intersection))
			{
				ranges.Add(intersection);
				if (thisRange.End.CompareTo(otherRange.End) <= 0)
				{
					i++;
				}
				else
				{
					j++;
				}
			}
			else if (thisRange.IsStrictlyLess(otherRange))
			{
				i++;
			}
			else//otherRange is strictly less than thisRange
			{
				j++;
			}
		}
		return new DiscontinuousRange<T>(ranges);
	}

	public DiscontinuousRange<T> Subtract(DiscontinuousRange<T> other)
	{
		List<Range<T>> ranges = new();
		int i = 0;
		int j = 0;
		bool endedInside = false;
		T? nextStartingPoint = default;
		while (i < Count && j < other.Count)
		{
			Range<T> thisRange = this[i];
			Range<T> otherRange = other[j];
			if (thisRange.IsStrictlyLess(otherRange))
			{
				if (endedInside)
				{
					ranges.Add(new Range<T>(nextStartingPoint!, thisRange.End));
					endedInside = false;
					nextStartingPoint = default;
				}
				i++;
			}
			else if (thisRange.IsStrictlyGreater(otherRange))
			{
				j++;
			}
			else
			{
				if (thisRange.Start.IsLess(otherRange.Start))
				{
					if (endedInside)
					{
						ranges.Add(new Range<T>(nextStartingPoint!, otherRange.Start));
						endedInside = false;
						nextStartingPoint = default;
					}
					else
					{
						ranges.Add(new Range<T>(thisRange.Start, otherRange.Start));
					}
				}
				if (thisRange.End.IsLessEqual(otherRange.End))
				{
					i++;
				}
				else
				{
					if (j < other.Count - 1)
					{
						endedInside = true;
						nextStartingPoint = otherRange.End;
					}
					else
					{
						ranges.Add(new Range<T>(otherRange.End, thisRange.End));
					}
					j++;
				}
			}
		}
		return new DiscontinuousRange<T>(ranges);
	}

	public override string ToString()
	{
		if (Count == 0)
		{
			return "Empty";
		}
		else
		{
			StringBuilder sb = new();
			sb.Append(this[0]);
			for (int i = 1; i < Count; i++)
			{
				sb.Append(", ");
				sb.Append(this[i]);
			}
			return sb.ToString();
		}
	}
}
