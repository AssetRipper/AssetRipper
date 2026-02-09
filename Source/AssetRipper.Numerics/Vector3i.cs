namespace AssetRipper.Numerics;

public struct Vector3i : IEquatable<Vector3i>
{
	public int X;
	public int Y;
	public int Z;

	public Vector3i(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public int GetValueByMember(int member)
	{
		member %= 3;
		if (member == 0)
		{
			return X;
		}

		if (member == 1)
		{
			return Y;
		}

		return Z;
	}

	public int GetMemberByValue(int value)
	{
		if (X == value)
		{
			return 0;
		}

		if (Y == value)
		{
			return 1;
		}

		if (Z == value)
		{
			return 2;
		}

		throw new ArgumentException($"Member with value {value} wasn't found");
	}

	public bool ContainsValue(int value)
	{
		if (X == value || Y == value || Z == value)
		{
			return true;
		}

		return false;
	}

	public override string ToString()
	{
		return $"[{X}, {Y}, {Z}]";
	}

	public override bool Equals(object? obj)
	{
		return obj is Vector3i i && Equals(i);
	}

	public bool Equals(Vector3i other)
	{
		return X == other.X && Y == other.Y && Z == other.Z;
	}

	public static bool operator ==(Vector3i left, Vector3i right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Vector3i left, Vector3i right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y, Z);
	}

	public static Vector3i Zero => default;
}
