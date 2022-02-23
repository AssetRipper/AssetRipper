using AssetRipper.Core.Math.Vectors;
using NUnit.Framework;

namespace AssetRipper.Tests
{
	public class QuaternionTests
	{
		[Test]
		public void ConvertingToQuaternionAndBackGivesTheSameValues()
		{
			//Many sets of values give exactly equal results
			Vector3f euler1 = new Vector3f(-67f, 45f, -162f);
			Vector3f converted1 = ConvertEulerToQuaternionAndBackToEuler(euler1);
			Assert.AreEqual(euler1, converted1);

			//Some however are only near equal due to rounding errors
			Vector3f euler2 = new Vector3f(-67f, 45f, 178f);
			Vector3f converted2 = ConvertEulerToQuaternionAndBackToEuler(euler2);
			Assert.IsTrue(euler2.IsEqualByDot(converted2));
		}

		private static Vector3f ConvertEulerToQuaternionAndBackToEuler(Vector3f original)
		{
			Quaternionf quaternion = original.ToQuaternion(true);
			return quaternion.ToEulerAngle(true);
		}

		[Test]
		public void ToQuaternionCreatesUnitQuaternions()
		{
			Vector3f euler = new Vector3f(-67f, 45f, -182f);
			Quaternionf quaternion = euler.ToQuaternion(true);
			Assert.IsTrue(quaternion.IsUnitQuaternion());
		}
	}
}
