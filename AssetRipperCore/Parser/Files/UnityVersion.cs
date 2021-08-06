using AssetRipper.Core.Extensions;
using System;
using System.IO;

namespace AssetRipper.Core.Parser.Files
{
	public readonly struct UnityVersion : IEquatable<UnityVersion>, IComparable, IComparable<UnityVersion>
	{
		private readonly ulong m_data;

		#region Constructors
		public UnityVersion(int major)
		{
			m_data = (ulong)(major & 0xFFFF) << 48;
		}

		public UnityVersion(int major, int minor)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40);
		}

		public UnityVersion(int major, int minor, int build)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32);
		}

		public UnityVersion(int major, int minor, int build, UnityVersionType type)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32)
				| ((ulong)((int)type & 0xFF) << 24);
		}

		public UnityVersion(int major, int minor, int build, UnityVersionType type, int typeNumber)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32)
				| ((ulong)((int)type & 0xFF) << 24) | ((ulong)(typeNumber & 0xFF) << 16);
		}

		private UnityVersion(ulong data)
		{
			m_data = data;
		}
		#endregion

		#region Properties
		public static UnityVersion MinVersion => new UnityVersion(0UL);
		public static UnityVersion MaxVersion => new UnityVersion(ulong.MaxValue);

		public int Major => unchecked((int)((m_data >> 48) & 0xFFFFUL));
		public int Minor => unchecked((int)((m_data >> 40) & 0xFFUL));
		public int Build => unchecked((int)((m_data >> 32) & 0xFFUL));
		public UnityVersionType Type => (UnityVersionType)unchecked((int)((m_data >> 24) & 0xFFUL));
		public int TypeNumber => unchecked((int)((m_data >> 16) & 0xFFUL));
		#endregion

		#region Miscellaneous Methods
		public int[] ToArray() => new int[] { Major, Minor, Build };

		public override string ToString()
		{
			return $"{Major}.{Minor}.{Build}{Type.ToLiteral()}{TypeNumber}";
		}

		public static UnityVersion Parse(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				throw new Exception($"Invalid version number {version}");
			}

			int major = 0;
			int minor = 0;
			int build = 0;
			UnityVersionType versionType = UnityVersionType.Final;
			int typeNumber = 0;
			using (StringReader reader = new StringReader(version))
			{
				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						throw new Exception($"Invalid version format");
					}
					char c = (char)symb;
					if (c == '.')
					{
						break;
					}

					major = major * 10 + c.ParseDigit();
				}

				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						break;
					}
					char c = (char)symb;
					if (c == '.')
					{
						break;
					}

					minor = minor * 10 + c.ParseDigit();
				}

				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						break;
					}

					char c = (char)symb;
					if (char.IsDigit(c))
					{
						build = build * 10 + c.ParseDigit();
					}
					else
					{
						switch (c)
						{
							case 'a':
								versionType = UnityVersionType.Alpha;
								break;

							case 'b':
								versionType = UnityVersionType.Beta;
								break;

							case 'p':
								versionType = UnityVersionType.Patch;
								break;

							case 'f':
								versionType = UnityVersionType.Final;
								break;

							default:
								throw new Exception($"Unsupported version type {c} for version '{version}'");
						}
						break;
					}
				}

				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						break;
					}

					char c = (char)symb;
					typeNumber = typeNumber * 10 + c.ParseDigit();
				}
			}
			return new UnityVersion(major, minor, build, versionType, typeNumber);
		}
		#endregion

		#region Comparison Operators
		public static bool operator ==(UnityVersion left, UnityVersion right)
		{
			return left.m_data == right.m_data;
		}

		public static bool operator !=(UnityVersion left, UnityVersion right)
		{
			return left.m_data != right.m_data;
		}

		public static bool operator >(UnityVersion left, UnityVersion right)
		{
			return left.m_data > right.m_data;
		}

		public static bool operator >=(UnityVersion left, UnityVersion right)
		{
			return left.m_data >= right.m_data;
		}

		public static bool operator <(UnityVersion left, UnityVersion right)
		{
			return left.m_data < right.m_data;
		}

		public static bool operator <=(UnityVersion left, UnityVersion right)
		{
			return left.m_data <= right.m_data;
		}
		#endregion

		#region IEquatable and IComparable
		public int CompareTo(object obj)
		{
			if (obj is UnityVersion version)
				return CompareTo(version);
			else
				return 1;
		}

		public int CompareTo(UnityVersion other)
		{
			if (this > other)
				return 1;
			else if (this < other)
				return -1;
			else
				return 0;
		}

		public override bool Equals(object obj)
		{
			if (obj is UnityVersion version) 
				return this == version;
			else 
				return false;
		}

		public bool Equals(UnityVersion other) => this == other;

		public override int GetHashCode()
		{
			return unchecked(827 + 911 * m_data.GetHashCode());
		}
		#endregion

		#region Comparison Methods
		public bool IsEqual(int major) => this == From(major);
		public bool IsEqual(int major, int minor) => this == From(major, minor);
		public bool IsEqual(int major, int minor, int build) => this == From(major, minor, build);
		public bool IsEqual(int major, int minor, int build, UnityVersionType type) => this == From(major, minor, build, type);
		public bool IsEqual(int major, int minor, int build, UnityVersionType type, int typeNumber) => this == new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsEqual(string version) => this == Parse(version);

		public bool IsLess(int major) => this < From(major);
		public bool IsLess(int major, int minor) => this < From(major, minor);
		public bool IsLess(int major, int minor, int build) => this < From(major, minor, build);
		public bool IsLess(int major, int minor, int build, UnityVersionType type) => this < From(major, minor, build, type);
		public bool IsLess(int major, int minor, int build, UnityVersionType type, int typeNumber) => this < new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsLess(string version) => this < Parse(version);

		public bool IsLessEqual(int major) => this <= From(major);
		public bool IsLessEqual(int major, int minor) => this <= From(major, minor);
		public bool IsLessEqual(int major, int minor, int build) => this <= From(major, minor, build);
		public bool IsLessEqual(int major, int minor, int build, UnityVersionType type) => this <= From(major, minor, build, type);
		public bool IsLessEqual(int major, int minor, int build, UnityVersionType type, int typeNumber) => this <= new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsLessEqual(string version) => this <= Parse(version);

		public bool IsGreater(int major) => this > From(major);
		public bool IsGreater(int major, int minor) => this > From(major, minor);
		public bool IsGreater(int major, int minor, int build) => this > From(major, minor, build);
		public bool IsGreater(int major, int minor, int build, UnityVersionType type) => this > From(major, minor, build, type);
		public bool IsGreater(int major, int minor, int build, UnityVersionType type, int typeNumber) => this > new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsGreater(string version) => this > Parse(version);

		public bool IsGreaterEqual(int major) => this >= From(major);
		public bool IsGreaterEqual(int major, int minor) => this >= From(major, minor);
		public bool IsGreaterEqual(int major, int minor, int build) => this >= From(major, minor, build);
		public bool IsGreaterEqual(int major, int minor, int build, UnityVersionType type) => this >= From(major, minor, build, type);
		public bool IsGreaterEqual(int major, int minor, int build, UnityVersionType type, int typeNumber) => this >= new UnityVersion(major, minor, build, type, typeNumber);
		public bool IsGreaterEqual(string version) => this >= Parse(version);
		#endregion

		#region From
		private UnityVersion From(int major)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | (0x0000FFFFFFFFFFFFUL & m_data);
			return new UnityVersion(data);
		}
		private UnityVersion From(int major, int minor)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | (0x000000FFFFFFFFFFUL & m_data);
			return new UnityVersion(data);
		}
		private UnityVersion From(int major, int minor, int build)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32) |
				(0x00000000FFFFFFFFUL & m_data);
			return new UnityVersion(data);
		}
		private UnityVersion From(int major, int minor, int build, UnityVersionType type)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32) |
				((ulong)((int)type & 0xFF) << 24) | (0x0000000000FFFFFFUL & m_data);
			return new UnityVersion(data);
		}
		private UnityVersion From(int major, int minor, int build, UnityVersionType type, int typeNumber)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32)
				| ((ulong)((int)type & 0xFF) << 24) | ((ulong)(typeNumber & 0xFF) << 16) | (0x000000000000FFFFUL & m_data);
			return new UnityVersion(data);
		}
		#endregion
	}
}
