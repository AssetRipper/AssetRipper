using AssetRipper.Core.Equality;

namespace AssetRipper.Tests
{
	public class NearEqualityTests
	{
		const float oneHundredth = .01f;
		private const float randomConstantFloat = 12.5f;
		private const double randomConstantDouble = randomConstantFloat;

		[Test]
		public void NanConstantReturnsFalse()
		{
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(float.NaN, randomConstantFloat, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(float.NaN, randomConstantFloat, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(double.NaN, randomConstantDouble, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(double.NaN, randomConstantDouble, oneHundredth));
		}

		[Test]
		public void NanNanReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(float.NaN, float.NaN, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(float.NaN, float.NaN, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(double.NaN, double.NaN, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(double.NaN, double.NaN, oneHundredth));
		}

		[Test]
		public void PosInfinConstantReturnsFalse()
		{
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(float.PositiveInfinity, randomConstantFloat, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(float.PositiveInfinity, randomConstantFloat, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(double.PositiveInfinity, randomConstantDouble, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(double.PositiveInfinity, randomConstantDouble, oneHundredth));
		}

		[Test]
		public void PosInfinPosInfinReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(float.PositiveInfinity, float.PositiveInfinity, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(float.PositiveInfinity, float.PositiveInfinity, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(double.PositiveInfinity, double.PositiveInfinity, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(double.PositiveInfinity, double.PositiveInfinity, oneHundredth));
		}

		[Test]
		public void NegInfinConstantReturnsFalse()
		{
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(float.NegativeInfinity, randomConstantFloat, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(float.NegativeInfinity, randomConstantFloat, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(double.NegativeInfinity, randomConstantDouble, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(double.NegativeInfinity, randomConstantDouble, oneHundredth));
		}

		[Test]
		public void NegInfinNegInfinReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(float.NegativeInfinity, float.NegativeInfinity, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(float.NegativeInfinity, float.NegativeInfinity, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(double.NegativeInfinity, double.NegativeInfinity, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(double.NegativeInfinity, double.NegativeInfinity, oneHundredth));
		}

		[Test]
		public void ProportionZeroConstantReturnsFalse()
		{
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(0f, randomConstantFloat, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(0d, randomConstantDouble, oneHundredth));
		}

		[Test]
		public void ZeroZeroReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(0f, 0f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(0f, 0f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(0d, 0d, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(0d, 0d, oneHundredth));
		}

		[Test]
		public void Proportion100WithAlmost101ReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(100f, 100.9999f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(100d, 100.9999d, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(100.9999f, 100f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(100.9999d, 100d, oneHundredth));
		}

		[Test]
		public void Proportion100With100ReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(100f, 100f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByProportion(100d, 100d, oneHundredth));
		}

		[Test]
		public void Proportion100With99ReturnsFalse()
		{
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(100f, 99f, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(100d, 99d, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(99f, 100f, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(99d, 100d, oneHundredth));
		}

		[Test]
		public void ProportionNeg1WithPos1ReturnsFalse()
		{
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(1f, -1f, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(1d, -1d, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(-1f, 1f, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByProportion(-1d, 1d, oneHundredth));
		}

		[Test]
		public void Deviation1With1_02ReturnsFalse()
		{
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(1f, 1.02f, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(1d, 1.02d, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(1.02f, 1f, oneHundredth));
			Assert.IsFalse(NearEquality.AlmostEqualByDeviation(1.02d, 1d, oneHundredth));
		}

		[Test]
		public void Deviation1WithAlmost1_01ReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(1f, 1.0099f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(1d, 1.0099d, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(1.0099f, 1f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(1.0099d, 1d, oneHundredth));
		}

		[Test]
		public void Deviation0_995WithAlmost1_005ReturnsTrue()
		{
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(0.995f, 1.00499f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(0.995d, 1.00499d, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(1.00499f, 0.995f, oneHundredth));
			Assert.IsTrue(NearEquality.AlmostEqualByDeviation(1.00499d, 0.995d, oneHundredth));
		}
	}
}
