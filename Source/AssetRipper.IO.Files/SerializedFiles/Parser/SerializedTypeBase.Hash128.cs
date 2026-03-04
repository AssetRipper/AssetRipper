using AssetRipper.IO.Files.SerializedFiles.IO;
using System.Runtime.CompilerServices;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

public abstract partial class SerializedTypeBase
{
	/// <summary>
	/// Hash128
	/// </summary>
	[InlineArray(4)]
	public struct Hash128 : IEquatable<Hash128>
	{
		private uint _element0;

		public Hash128(uint element0, uint element1, uint element2, uint element3)
		{
			this[0] = element0;
			this[1] = element1;
			this[2] = element2;
			this[3] = element3;
		}

		internal static Hash128 Read(SerializedReader reader)
		{
			return new Hash128(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
		}

		internal readonly void Write(SerializedWriter writer)
		{
			writer.Write(this[0]);
			writer.Write(this[1]);
			writer.Write(this[2]);
			writer.Write(this[3]);
		}

		public override readonly bool Equals(object? obj)
		{
			return obj is Hash128 hash && Equals(hash);
		}

		public readonly bool Equals(Hash128 other)
		{
			return ((ReadOnlySpan<uint>)this).SequenceEqual(other);
		}

		public override readonly int GetHashCode()
		{
			return HashCode.Combine(this[0], this[1], this[2], this[3]);
		}

		public static bool operator ==(Hash128 left, Hash128 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Hash128 left, Hash128 right)
		{
			return !(left == right);
		}
	}
}
