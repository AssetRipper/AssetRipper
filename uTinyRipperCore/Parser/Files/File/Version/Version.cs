using System;
using System.IO;

namespace uTinyRipper
{
	public readonly struct Version : IComparable<Version>
	{
		public Version(int major)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48);
		}

		public Version(int major, int minor)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40);
		}

		public Version(int major, int minor, int build)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32);
		}

		public Version(int major, int minor, int build, VersionType type)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32)
				| ((ulong)((int)type & 0xFF) << 24);
		}

		public Version(int major, int minor, int build, VersionType type, int typeNumber)
		{
			m_data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32)
				| ((ulong)((int)type & 0xFF) << 24) | ((ulong)(typeNumber & 0xFF) << 16);
		}

		private Version(ulong data)
		{
			m_data = data;
		}

		public static bool operator ==(Version left, Version right)
		{
			return left.m_data == right.m_data;
		}

		public static bool operator !=(Version left, Version right)
		{
			return left.m_data != right.m_data;
		}

		public static bool operator >(Version left, Version right)
		{
			return left.m_data > right.m_data;
		}

		public static bool operator >=(Version left, Version right)
		{
			return left.m_data >= right.m_data;
		}

		public static bool operator <(Version left, Version right)
		{
			return left.m_data < right.m_data;
		}

		public static bool operator <=(Version left, Version right)
		{
			return left.m_data <= right.m_data;
		}

		public static Version Parse(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				throw new Exception($"Invalid version number {version}");
			}

			int major = 0;
			int minor = 0;
			int build = 0;
			VersionType versionType = VersionType.Final;
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
								versionType = VersionType.Alpha;
								break;

							case 'b':
								versionType = VersionType.Beta;
								break;

							case 'p':
								versionType = VersionType.Patch;
								break;

							case 'f':
								versionType = VersionType.Final;
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
			return new Version(major, minor, build, versionType, typeNumber);
		}

		public int CompareTo(Version other)
		{
			if (this > other)
			{
				return 1;
			}
			else if (this < other)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Version))
			{
				return false;
			}
			return this == (Version)obj;
		}

		public override int GetHashCode()
		{
			return unchecked(827 + 911 * m_data.GetHashCode());
		}

		public override string ToString()
		{
			return $"{Major}.{Minor}.{Build}{Type.ToLiteral()}{TypeNumber}";
		}

		public bool IsEqual(int major)
		{
			return this == From(major);
		}

		public bool IsEqual(int major, int minor)
		{
			return this == From(major, minor);
		}

		public bool IsEqual(int major, int minor, int build)
		{
			return this == From(major, minor, build);
		}

		public bool IsEqual(int major, int minor, int build, VersionType type)
		{
			return this == From(major, minor, build, type);
		}

		public bool IsEqual(int major, int minor, int build, VersionType type, int typeNumber)
		{
			return this == new Version(major, minor, build, type, typeNumber);
		}

		public bool IsEqual(string version)
		{
			return this == Parse(version);
		}

		public bool IsLess(int major)
		{
			return this < From(major);
		}

		public bool IsLess(int major, int minor)
		{
			return this < From(major, minor);
		}

		public bool IsLess(int major, int minor, int build)
		{
			return this < From(major, minor, build);
		}

		public bool IsLess(int major, int minor, int build, VersionType type)
		{
			return this < From(major, minor, build, type);
		}

		public bool IsLess(int major, int minor, int build, VersionType type, int typeNumber)
		{
			return this < new Version(major, minor, build, type, typeNumber);
		}

		public bool IsLess(string version)
		{
			return this < Parse(version);
		}

		public bool IsLessEqual(int major)
		{
			return this <= From(major);
		}

		public bool IsLessEqual(int major, int minor)
		{
			return this <= From(major, minor);
		}

		public bool IsLessEqual(int major, int minor, int build)
		{
			return this <= From(major, minor, build);
		}

		public bool IsLessEqual(int major, int minor, int build, VersionType type)
		{
			return this <= From(major, minor, build, type);
		}

		public bool IsLessEqual(int major, int minor, int build, VersionType type, int typeNumber)
		{
			return this <= new Version(major, minor, build, type, typeNumber);
		}

		public bool IsLessEqual(string version)
		{
			return this <= Parse(version);
		}

		public bool IsGreater(int major)
		{
			return this > From(major);
		}

		public bool IsGreater(int major, int minor)
		{
			return this > From(major, minor);
		}

		public bool IsGreater(int major, int minor, int build)
		{
			return this > From(major, minor, build);
		}

		public bool IsGreater(int major, int minor, int build, VersionType type)
		{
			return this > From(major, minor, build, type);
		}

		public bool IsGreater(int major, int minor, int build, VersionType type, int typeNumber)
		{
			return this > new Version(major, minor, build, type, typeNumber);
		}

		public bool IsGreater(string version)
		{
			return this > Parse(version);
		}

		public bool IsGreaterEqual(int major)
		{
			return this >= From(major);
		}

		public bool IsGreaterEqual(int major, int minor)
		{
			return this >= From(major, minor);
		}

		public bool IsGreaterEqual(int major, int minor, int build)
		{
			return this >= From(major, minor, build);
		}

		public bool IsGreaterEqual(int major, int minor, int build, VersionType type)
		{
			return this >= From(major, minor, build, type);
		}

		public bool IsGreaterEqual(int major, int minor, int build, VersionType type, int typeNumber)
		{
			return this >= new Version(major, minor, build, type, typeNumber);
		}

		public bool IsGreaterEqual(string version)
		{
			return this >= Parse(version);
		}

		private Version From(int major)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | (0x0000FFFFFFFFFFFFUL & m_data);
			return new Version(data);
		}
		private Version From(int major, int minor)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | (0x000000FFFFFFFFFFUL & m_data);
			return new Version(data);
		}
		private Version From(int major, int minor, int build)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32) |
				(0x00000000FFFFFFFFUL & m_data);
			return new Version(data);
		}
		private Version From(int major, int minor, int build, VersionType type)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32) |
				((ulong)((int)type & 0xFF) << 24) | (0x0000000000FFFFFFUL & m_data);
			return new Version(data);
		}
		private Version From(int major, int minor, int build, VersionType type, int typeNumber)
		{
			ulong data = ((ulong)(major & 0xFFFF) << 48) | ((ulong)(minor & 0xFF) << 40) | ((ulong)(build & 0xFF) << 32)
				| ((ulong)((int)type & 0xFF) << 24) | ((ulong)(typeNumber & 0xFF) << 16) | (0x000000000000FFFFUL & m_data);
			return new Version(data);
		}

		public static Version MinVersion => new Version(0UL);
		public static Version MaxVersion => new Version(ulong.MaxValue);

		public int Major
		{
			get => unchecked((int)((m_data >> 48) & 0xFFFFUL));
		}
		public int Minor
		{
			get => unchecked((int)((m_data >> 40) & 0xFFUL));
		}
		public int Build
		{
			get => unchecked((int)((m_data >> 32) & 0xFFUL));
		}
		public VersionType Type
		{
			get => (VersionType)unchecked((int)((m_data >> 24) & 0xFFUL));
		}
		public int TypeNumber
		{
			get => unchecked((int)((m_data >> 16) & 0xFFUL));
		}

		private readonly ulong m_data;
	}
}
