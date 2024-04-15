namespace AssetRipper.Numerics.Tests;

public class DiscontinuousRangeTests
{
	private static readonly Range<float> MinToZero = new Range<float>(float.MinValue, 0);
	private static readonly Range<float> ZeroToThree = new Range<float>(0, 3);
	private static readonly Range<float> ThreeToFour = new Range<float>(3, 4);
	private static readonly Range<float> FourToFive = new Range<float>(4, 5);
	private static readonly Range<float> FiveToSeven = new Range<float>(5, 7);
	private static readonly Range<float> SevenToNine = new Range<float>(7, 9);
	private static readonly Range<float> NineToTen = new Range<float>(9, 10);
	private static readonly Range<float> ZeroToTen = new Range<float>(0, 110);
	private static readonly Range<float> ZeroToEleven = new Range<float>(0, 11);
	private static readonly Range<float> TenToTwenty = new Range<float>(10, 20);
	private static readonly Range<float> ZeroToTwenty = new Range<float>(0, 20);
	private static readonly Range<float> ZeroToMax = new Range<float>(0, float.MaxValue);
	private static readonly Range<float> TwentyToMax = new Range<float>(20, float.MaxValue);
	private static readonly Range<float> MinToMax = new Range<float>(float.MinValue, float.MaxValue);

	[Test]
	public void DisjointConstructionSucceedsAndHasCorrectCount()
	{
		var range = new DiscontinuousRange<float>(ZeroToThree, SevenToNine, FourToFive);
		Assert.That(range.Count, Is.EqualTo(3));
	}

	[Test]
	public void OverlappingConstructionSucceedsAndHasCorrectCount()
	{
		var range = new DiscontinuousRange<float>(ZeroToThree, SevenToNine, ZeroToEleven, FourToFive);
		Assert.That(range.Count, Is.EqualTo(1));
	}

	[Test]
	public void EmptyHasCountZero()
	{
		Assert.That(DiscontinuousRange<float>.Empty.Count, Is.EqualTo(0));
	}

	[Test]
	public void CommutativeConstruction()
	{
		var range1 = new DiscontinuousRange<float>(ZeroToThree, SevenToNine, FourToFive);
		var range2 = new DiscontinuousRange<float>(SevenToNine, FourToFive, ZeroToThree);
		AssertEqual(range1, range2);
	}

	[Test]
	public void MergingConstruction()
	{
		var range1 = new DiscontinuousRange<float>(TenToTwenty, TwentyToMax, ZeroToTen);
		var expected = new DiscontinuousRange<float>(ZeroToMax);
		AssertEqual(range1, expected);
	}

	[Test]
	public void UnionTest()
	{
		var range1 = new DiscontinuousRange<float>(TwentyToMax, ZeroToTen);
		var range2 = new DiscontinuousRange<float>(ZeroToTwenty);
		var expected = new DiscontinuousRange<float>(ZeroToMax);
		AssertEqual(range1.Union(range2), expected);
	}

	[Test]
	public void NegationTest()
	{
		var range1 = new DiscontinuousRange<float>(MinToZero);
		var expected = new DiscontinuousRange<float>(ZeroToMax);
		AssertEqual(range1.Negate(float.MinValue, float.MaxValue), expected);
	}

	[Test]
	public void SubtractMiddleTest()
	{
		var range1 = new DiscontinuousRange<float>(MinToMax);
		var expected = new DiscontinuousRange<float>(MinToZero, TwentyToMax);
		AssertEqual(range1.Subtract(ZeroToTwenty), expected);
	}

	[Test]
	public void SubtractLeftTest()
	{
		var range1 = new DiscontinuousRange<float>(MinToMax);
		var expected = new DiscontinuousRange<float>(ZeroToMax);
		AssertEqual(range1.Subtract(MinToZero), expected);
	}

	[Test]
	public void SubtractRightTest()
	{
		var range1 = new DiscontinuousRange<float>(MinToMax);
		var expected = new DiscontinuousRange<float>(MinToZero);
		AssertEqual(range1.Subtract(ZeroToMax), expected);
	}

	[Test]
	public void SubtractAllTest()
	{
		var range1 = new DiscontinuousRange<float>(TenToTwenty);
		var expected = DiscontinuousRange<float>.Empty;
		AssertEqual(range1.Subtract(ZeroToMax), expected);
	}

	[Test]
	public void SubtractLeftOverlappingTest()
	{
		var range1 = new DiscontinuousRange<float>(ZeroToMax);
		var range2 = new DiscontinuousRange<float>(MinToZero, ZeroToTwenty);
		var expected = new DiscontinuousRange<float>(TwentyToMax);
		AssertEqual(range1.Subtract(range2), expected);
	}

	[Test]
	public void SubtractEndsTest()
	{
		var range1 = new DiscontinuousRange<float>(ZeroToMax);
		var range2 = new DiscontinuousRange<float>(MinToZero, TwentyToMax);
		var expected = new DiscontinuousRange<float>(ZeroToTwenty);
		AssertEqual(range1.Subtract(range2), expected);
	}

	[Test]
	public void SubtractManyTest()
	{
		var range1 = new DiscontinuousRange<float>(ZeroToMax);
		var range2 = new DiscontinuousRange<float>(MinToZero, ZeroToThree, ThreeToFour, FiveToSeven, SevenToNine, TenToTwenty);
		var expected = new DiscontinuousRange<float>(FourToFive, NineToTen, TwentyToMax);
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
