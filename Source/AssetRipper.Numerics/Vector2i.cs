namespace AssetRipper.Numerics;

public struct Vector2i : IEquatable<Vector2i>
{
	public int X;
	public int Y;

	public Vector2i()
	{
		X = 0;
		Y = 0;
	}

	public Vector2i(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return $"[{X}, {Y}]";
	}

	public override bool Equals(object? obj)
	{
		return obj is Vector2i i && Equals(i);
	}

	public bool Equals(Vector2i other)
	{
		return X == other.X && Y == other.Y;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y);
	}

	public static bool operator ==(Vector2i left, Vector2i right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Vector2i left, Vector2i right)
	{
		return !(left == right);
	}
}
