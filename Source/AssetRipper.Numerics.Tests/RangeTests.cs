namespace AssetRipper.Numerics.Tests;

public class RangeTests
{
	private static readonly Range<int> ZeroToTen = new Range<int>(0, 10);
	private static readonly Range<int> OneToTen = new Range<int>(1, 10);
	private static readonly Range<int> OneToEleven = new Range<int>(1, 11);
	private static readonly Range<int> ZeroToEleven = new Range<int>(0, 11);
	private static readonly Range<int> TenToTwenty = new Range<int>(10, 20);
	private static readonly Range<int> ZeroToTwenty = new Range<int>(0, 20);

	[Test]
	public void IntersectionTest1()
	{
		Assert.That(OneToTen, Is.EqualTo(ZeroToTen.MakeIntersection(OneToEleven)));
	}

	[Test]
	public void IntersectingUnionTest()
	{
		Assert.That(ZeroToEleven, Is.EqualTo(ZeroToTen.MakeUnion(OneToEleven)));
	}

	[Test]
	public void NonintersectingUnionTest()
	{
		Assert.That(ZeroToTwenty, Is.EqualTo(ZeroToTen.MakeUnion(TenToTwenty)));
	}

	[Test]
	public void ContainsItself()
	{
		Assert.That(ZeroToTen.Contains(ZeroToTen));
	}

	[Test]
	public void ContainsStart()
	{
		Assert.That(ZeroToTen.Contains(0));
	}

	[Test]
	public void ContainsMiddle()
	{
		Assert.That(ZeroToTen.Contains(5));
	}

	[Test]
	public void DoesNotContainEnd()
	{
		Assert.That(!ZeroToTen.Contains(10));
	}

	[Test]
	public void DoesNotContainLess()
	{
		Assert.That(!ZeroToTen.Contains(-10));
	}

	[Test]
	public void DoesNotContainMore()
	{
		Assert.That(!ZeroToTen.Contains(100));
	}

	[Test]
	public void EndBeforeStartThrows()
	{
		Assert.Throws<ArgumentException>(() => new Range<int>(4, 3));
	}

	[Test]
	public void EndEqualsStartThrows()
	{
		Assert.Throws<ArgumentException>(() => new Range<int>(4, 4));
	}

	[Test]
	public void StrictComparisons()
	{
		using (Assert.EnterMultipleScope())
		{
			//Correct
			Assert.That(ZeroToTen.IsStrictlyLess(TenToTwenty));
			Assert.That(TenToTwenty.IsStrictlyGreater(ZeroToTen));

			//Reversed
			Assert.That(!TenToTwenty.IsStrictlyLess(ZeroToTen));
			Assert.That(!ZeroToTen.IsStrictlyGreater(TenToTwenty));

			//Overlapping
			Assert.That(!TenToTwenty.IsStrictlyLess(ZeroToEleven));
			Assert.That(!ZeroToEleven.IsStrictlyLess(TenToTwenty));
			Assert.That(!TenToTwenty.IsStrictlyGreater(ZeroToEleven));
			Assert.That(!ZeroToEleven.IsStrictlyGreater(TenToTwenty));
		}
	}
}
