namespace AssetRipper.Numerics.Tests;

public class QuaternionTests
{
	[Test]
	public void ConvertingToQuaternionAndBackGivesTheSameValues()
	{
		//Many sets of values give exactly equal results
		Vector3 euler1 = new Vector3(-67f, 45f, -162f);
		Vector3 converted1 = ConvertEulerToQuaternionAndBackToEuler(euler1);
		Assert.That(converted1, Is.EqualTo(euler1));

		//Some however are only near equal due to rounding errors
		Vector3 euler2 = new Vector3(-67f, 45f, 178f);
		Vector3 converted2 = ConvertEulerToQuaternionAndBackToEuler(euler2);
		Assert.That(euler2.EqualsByDot(converted2));
	}

	private static Vector3 ConvertEulerToQuaternionAndBackToEuler(Vector3 original)
	{
		Quaternion quaternion = original.ToQuaternion(true);
		return quaternion.ToEulerAngle(true);
	}

	[Test]
	public void ToQuaternionCreatesUnitQuaternions()
	{
		Vector3 euler = new Vector3(-67f, 45f, -182f);
		Quaternion quaternion = euler.ToQuaternion(true);
		Assert.That(quaternion.IsUnitQuaternion());
	}
}
