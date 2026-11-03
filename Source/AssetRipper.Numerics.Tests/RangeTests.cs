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
	public async Task IntersectionTest1()
	{
		await Assert.That(OneToTen).IsEqualTo(ZeroToTen.MakeIntersection(OneToEleven));
	}

	[Test]
	public async Task IntersectingUnionTest()
	{
		await Assert.That(ZeroToEleven).IsEqualTo(ZeroToTen.MakeUnion(OneToEleven));
	}

	[Test]
	public async Task NonintersectingUnionTest()
	{
		await Assert.That(ZeroToTwenty).IsEqualTo(ZeroToTen.MakeUnion(TenToTwenty));
	}

	[Test]
	public async Task ContainsItself()
	{
		await Assert.That(ZeroToTen.Contains(ZeroToTen)).IsTrue();
	}

	[Test]
	public async Task ContainsStart()
	{
		await Assert.That(ZeroToTen.Contains(0)).IsTrue();
	}

	[Test]
	public async Task ContainsMiddle()
	{
		await Assert.That(ZeroToTen.Contains(5)).IsTrue();
	}

	[Test]
	public async Task DoesNotContainEnd()
	{
		await Assert.That(!ZeroToTen.Contains(10)).IsTrue();
	}

	[Test]
	public async Task DoesNotContainLess()
	{
		await Assert.That(!ZeroToTen.Contains(-10)).IsTrue();
	}

	[Test]
	public async Task DoesNotContainMore()
	{
		await Assert.That(!ZeroToTen.Contains(100)).IsTrue();
	}

	[Test]
	public async Task EndBeforeStartThrows()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => Task.Run(() => new Range<int>(4, 3)));
	}

	[Test]
	public async Task EndEqualsStartThrows()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => Task.Run(() => new Range<int>(4, 4)));
	}

	[Test]
	public async Task StrictComparisons()
	{

		{
			//Correct
			await Assert.That(ZeroToTen.IsStrictlyLess(TenToTwenty)).IsTrue();
			await Assert.That(TenToTwenty.IsStrictlyGreater(ZeroToTen)).IsTrue();

			//Reversed
			await Assert.That(!TenToTwenty.IsStrictlyLess(ZeroToTen)).IsTrue();
			await Assert.That(!ZeroToTen.IsStrictlyGreater(TenToTwenty)).IsTrue();

			//Overlapping
			await Assert.That(!TenToTwenty.IsStrictlyLess(ZeroToEleven)).IsTrue();
			await Assert.That(!ZeroToEleven.IsStrictlyLess(TenToTwenty)).IsTrue();
			await Assert.That(!TenToTwenty.IsStrictlyGreater(ZeroToEleven)).IsTrue();
			await Assert.That(!ZeroToEleven.IsStrictlyGreater(TenToTwenty)).IsTrue();
		}
	}
}