namespace AssetRipper.Numerics;

public static class VectorExtensions
{
	public static Vector3 InvertX(this Vector3 vector)
	{
		return new Vector3(-vector.X, vector.Y, vector.Z);
	}
}
