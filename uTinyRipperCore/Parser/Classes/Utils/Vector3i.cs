using System;

namespace uTinyRipper.Classes
{

	public struct Vector3i
	{
		public Vector3i(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static bool operator == (Vector3i left, Vector3i right)
		{
			return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
		}

		public static bool operator !=(Vector3i left, Vector3i right)
		{
			return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
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

		public override bool Equals(object obj)
		{
			if(obj == null)
			{
				return false;
			}
			if(obj.GetType() != typeof(Vector3i))
			{
				return false;
			}
			return this == (Vector3i)obj;
		}

		public override int GetHashCode()
		{
			int hash = 193;
			unchecked
			{
				hash = hash + 787 * X.GetHashCode();
				hash = hash * 823 + Y.GetHashCode();
				hash = hash * 431 + Z.GetHashCode();
			}
			return hash;
		}

		public int X { get; }
		public int Y { get; }
		public int Z { get; }
	}
}
