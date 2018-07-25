using System;

namespace UtinyRipper.Classes
{

	public struct Vector3i
	{
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

		public int X { get; }
		public int Y { get; }
		public int Z { get; }
	}
}
