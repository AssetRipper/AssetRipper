namespace AssetRipper.Numerics.Tests;

public class DiscontinuousRangeTests
{
	private static readonly Range<float> MinToZero = new(float.MinValue, 0);
	private static readonly Range<float> ZeroToThree = new(0, 3);
	private static readonly Range<float> ThreeToFour = new(3, 4);
	private static readonly Range<float> FourToFive = new(4, 5);
	private static readonly Range<float> FiveToSeven = new(5, 7);
	private static readonly Range<float> SevenToNine = new(7, 9);
	private static readonly Range<float> NineToTen = new(9, 10);
	private static readonly Range<float> ZeroToTen = new(0, 110);
	private static readonly Range<float> ZeroToEleven = new(0, 11);
	private static readonly Range<float> TenToTwenty = new(10, 20);
	private static readonly Range<float> ZeroToTwenty = new(0, 20);
	private static readonly Range<float> ZeroToMax = new(0, float.MaxValue);
	private static readonly Range<float> TwentyToMax = new(20, float.MaxValue);
	private static readonly Range<float> MinToMax = new(float.MinValue, float.MaxValue);

	[Test]
	public void DisjointConstructionSucceedsAndHasCorrectCount()
	{
		DiscontinuousRange<float> range = new(ZeroToThree, SevenToNine, FourToFive);
		Assert.That(range, Has.Count.EqualTo(3));
	}

	[Test]
	public void OverlappingConstructionSucceedsAndHasCorrectCount()
	{
		DiscontinuousRange<float> range = new(ZeroToThree, SevenToNine, ZeroToEleven, FourToFive);
		Assert.That(range, Has.Count.EqualTo(1));
	}

	[Test]
	public void EmptyHasCountZero()
	{
		Assert.That(DiscontinuousRange<float>.Empty.Count, Is.Zero);
	}

	[Test]
	public void CommutativeConstruction()
	{
		DiscontinuousRange<float> range1 = new(ZeroToThree, SevenToNine, FourToFive);
		DiscontinuousRange<float> range2 = new(SevenToNine, FourToFive, ZeroToThree);
		AssertEqual(range1, range2);
	}

	[Test]
	public void MergingConstruction()
	{
		DiscontinuousRange<float> range1 = new(TenToTwenty, TwentyToMax, ZeroToTen);
		DiscontinuousRange<float> expected = new(ZeroToMax);
		AssertEqual(range1, expected);
	}

	[Test]
	public void UnionTest()
	{
		DiscontinuousRange<float> range1 = new(TwentyToMax, ZeroToTen);
		DiscontinuousRange<float> range2 = new(ZeroToTwenty);
		DiscontinuousRange<float> expected = new(ZeroToMax);
		AssertEqual(range1.Union(range2), expected);
	}

	[Test]
	public void NegationTest()
	{
		DiscontinuousRange<float> range1 = new(MinToZero);
		DiscontinuousRange<float> expected = new(ZeroToMax);
		AssertEqual(range1.Negate(float.MinValue, float.MaxValue), expected);
	}

	[Test]
	public void SubtractMiddleTest()
	{
		DiscontinuousRange<float> range1 = new(MinToMax);
		DiscontinuousRange<float> expected = new(MinToZero, TwentyToMax);
		AssertEqual(range1.Subtract(ZeroToTwenty), expected);
	}

	[Test]
	public void SubtractLeftTest()
	{
		DiscontinuousRange<float> range1 = new(MinToMax);
		DiscontinuousRange<float> expected = new(ZeroToMax);
		AssertEqual(range1.Subtract(MinToZero), expected);
	}

	[Test]
	public void SubtractRightTest()
	{
		DiscontinuousRange<float> range1 = new(MinToMax);
		DiscontinuousRange<float> expected = new(MinToZero);
		AssertEqual(range1.Subtract(ZeroToMax), expected);
	}

	[Test]
	public void SubtractAllTest()
	{
		DiscontinuousRange<float> range1 = new(TenToTwenty);
		DiscontinuousRange<float> expected = DiscontinuousRange<float>.Empty;
		AssertEqual(range1.Subtract(ZeroToMax), expected);
	}

	[Test]
	public void SubtractLeftOverlappingTest()
	{
		DiscontinuousRange<float> range1 = new(ZeroToMax);
		DiscontinuousRange<float> range2 = new(MinToZero, ZeroToTwenty);
		DiscontinuousRange<float> expected = new(TwentyToMax);
		AssertEqual(range1.Subtract(range2), expected);
	}

	[Test]
	public void SubtractEndsTest()
	{
		DiscontinuousRange<float> range1 = new(ZeroToMax);
		DiscontinuousRange<float> range2 = new(MinToZero, TwentyToMax);
		DiscontinuousRange<float> expected = new(ZeroToTwenty);
		AssertEqual(range1.Subtract(range2), expected);
	}

	[Test]
	public void SubtractManyTest()
	{
		DiscontinuousRange<float> range1 = new(ZeroToMax);
		DiscontinuousRange<float> range2 = new(MinToZero, ZeroToThree, ThreeToFour, FiveToSeven, SevenToNine, TenToTwenty);
		DiscontinuousRange<float> expected = new(FourToFive, NineToTen, TwentyToMax);
		AssertEqual(range1.Subtract(range2), expected);
	}

	/// <summary>
	/// Because NUnit refuses to call ToString()
	/// </summary>
	/// <param name="actual"></param>
	/// <param name="expected"></param>
	private static void AssertEqual<T>(DiscontinuousRange<T> actual, DiscontinuousRange<T> expected) where T : notnull, IComparable<T>, IEquatable<T>
	{
		if (!actual.Equals(expected))
		{
			Assert.Fail($"Expected: {expected}\nBut was: {actual}");
		}
	}
}
