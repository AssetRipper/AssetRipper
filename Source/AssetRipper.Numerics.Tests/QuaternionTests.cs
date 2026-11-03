namespace AssetRipper.Numerics.Tests;

public class QuaternionTests
{
	[Test]
	public async Task ConvertingToQuaternionAndBackGivesTheSameValues()
	{
		//Many sets of values give exactly equal results
		Vector3 euler1 = new Vector3(-67f, 45f, -162f);
		Vector3 converted1 = ConvertEulerToQuaternionAndBackToEuler(euler1);
		await Assert.That(converted1).IsEqualTo(euler1);

		//Some however are only near equal due to rounding errors
		Vector3 euler2 = new Vector3(-67f, 45f, 178f);
		Vector3 converted2 = ConvertEulerToQuaternionAndBackToEuler(euler2);
		await Assert.That(euler2.EqualsByDot(converted2)).IsTrue();
	}

	private static Vector3 ConvertEulerToQuaternionAndBackToEuler(Vector3 original)
	{
		Quaternion quaternion = original.ToQuaternion(true);
		return quaternion.ToEulerAngle(true);
	}

	[Test]
	public async Task ToQuaternionCreatesUnitQuaternions()
	{
		Vector3 euler = new Vector3(-67f, 45f, -182f);
		Quaternion quaternion = euler.ToQuaternion(true);
		await Assert.That(quaternion.IsUnitQuaternion()).IsTrue();
	}
}