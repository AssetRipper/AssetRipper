using System;
using System.IO;

namespace UtinyRipper
{
	public class Version
	{		
		public static bool operator ==(Version left, Version right)
		{
			if(ReferenceEquals(right, null))
			{
				return false;
			}

			if (left.Major != right.Major)
			{
				return false;
			}
			if (left.Minor != right.Minor)
			{
				return false;
			}
			if(left.Build != right.Build)
			{
				return false;
			}
			if (left.Type != right.Type)
			{
				return false;
			}
			if(left.TypeNumber != right.TypeNumber)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(Version left, Version right)
		{
			return !(left == right);
		}

		public static bool operator >(Version left, Version right)
		{
			if(left.Major > right.Major)
			{
				return true;
			}
			if(left.Major < right.Major)
			{
				return false;
			}

			if (left.Minor > right.Minor)
			{
				return true;
			}
			if (left.Minor < right.Minor)
			{
				return false;
			}

			if (left.Build > right.Build)
			{
				return true;
			}
			if (left.Build < right.Build)
			{
				return false;
			}

			if (left.Type > right.Type)
			{
				return true;
			}
			if (left.Type < right.Type)
			{
				return false;
			}

			if (left.TypeNumber > right.TypeNumber)
			{
				return true;
			}
			return false;
		}

		public static bool operator >=(Version left, Version right)
		{
			if (left.Major > right.Major)
			{
				return true;
			}
			if (left.Major < right.Major)
			{
				return false;
			}

			if (left.Minor > right.Minor)
			{
				return true;
			}
			if (left.Minor < right.Minor)
			{
				return false;
			}

			if (left.Build > right.Build)
			{
				return true;
			}
			if (left.Build < right.Build)
			{
				return false;
			}

			if (left.Type > right.Type)
			{
				return true;
			}
			if (left.Type < right.Type)
			{
				return false;
			}

			if (left.TypeNumber > right.TypeNumber)
			{
				return true;
			}
			if(left.TypeNumber < right.TypeNumber)
			{
				return false;
			}
			return true;
		}

		public static bool operator <(Version left, Version right)
		{
			if (left.Major < right.Major)
			{
				return true;
			}
			if (left.Major > right.Major)
			{
				return false;
			}

			if (left.Minor < right.Minor)
			{
				return true;
			}
			if (left.Minor > right.Minor)
			{
				return false;
			}

			if (left.Build < right.Build)
			{
				return true;
			}
			if (left.Build > right.Build)
			{
				return false;
			}

			if (left.Type < right.Type)
			{
				return true;
			}
			if (left.Type > right.Type)
			{
				return false;
			}

			if (left.TypeNumber < right.TypeNumber)
			{
				return true;
			}
			return false;
		}
		
		public static bool operator <=(Version left, Version right)
		{
			if (left.Major < right.Major)
			{
				return true;
			}
			if (left.Major > right.Major)
			{
				return false;
			}

			if (left.Minor < right.Minor)
			{
				return true;
			}
			if (left.Minor > right.Minor)
			{
				return false;
			}

			if (left.Build < right.Build)
			{
				return true;
			}
			if (left.Build > right.Build)
			{
				return false;
			}

			if (left.Type < right.Type)
			{
				return true;
			}
			if (left.Type > right.Type)
			{
				return false;
			}

			if (left.TypeNumber < right.TypeNumber)
			{
				return true;
			}
			if (left.TypeNumber > right.TypeNumber)
			{
				return false;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(obj, null))
			{
				return false;
			}
			if(typeof(Version) != obj.GetType())
			{
				return false;
			}
			return this == (Version)obj;
		}

		public override int GetHashCode()
		{
			int hash = Major.GetHashCode();
			unchecked
			{
				hash += 17 * Minor.GetHashCode();
				hash += 23 * Build.GetHashCode();
				hash += 29 * (int)Type;
				hash += 31 * TypeNumber;
			}
			return hash;
		}

		public override string ToString()
		{
			string result = $"{Major}.{Minor}.{Build}";
			if(Type != VersionType.Base)
			{
				result = $"{result}{Type.ToLiteral()}{TypeNumber}";
			}
			return result;
		}

		public bool IsEqual(int major)
		{
			return Major == major;
		}

		public bool IsEqual(int major, int minor)
		{
			return Major == major && Minor == minor;
		}

		public bool IsEqual(int major, int minor, int build)
		{
			return Major == major && Minor == minor && Build == build;
		}

		public bool IsEqual(int major, int minor, int build, VersionType type)
		{
			return Major == major && Minor == minor && Build == build && Type == type;
		}

		public bool IsEqual(int major, int minor, int build, VersionType type, int typeNumer)
		{
			return Major == major && Minor == minor && Build == build && Type == type && TypeNumber == typeNumer;
		}

		public bool IsEqual(string version)
		{
			Version compareVersion = new Version();
			compareVersion.Parse(version);
			return this == compareVersion;
		}

		public bool IsLess(int major)
		{
			return Major < major;
		}

		public bool IsLess(int major, int minor)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}
			return Minor < minor;
		}

		public bool IsLess(int major, int minor, int build)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}

			if (Minor < minor)
			{
				return true;
			}
			if (Minor > minor)
			{
				return false;
			}
			return Build < build;
		}

		public bool IsLess(int major, int minor, int build, VersionType type)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}

			if (Minor < minor)
			{
				return true;
			}
			if (Minor > minor)
			{
				return false;
			}

			if (Build < build)
			{
				return true;
			}
			if (Build > build)
			{
				return false;
			}

			return Type < type;
		}

		public bool IsLess(int major, int minor, int build, VersionType type, int typeNumber)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}

			if (Minor < minor)
			{
				return true;
			}
			if (Minor > minor)
			{
				return false;
			}

			if (Build < build)
			{
				return true;
			}
			if (Build > build)
			{
				return false;
			}

			if (Type < type)
			{
				return true;
			}
			if (Type > type)
			{
				return false;
			}

			return TypeNumber < typeNumber;
		}

		public bool IsLess(string version)
		{
			Version compareVersion = new Version();
			compareVersion.Parse(version);
			return this < compareVersion;
		}

		public bool IsLessEqual(int major)
		{
			return Major <= major;
		}

		public bool IsLessEqual(int major, int minor)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}
			return Minor <= minor;
		}

		public bool IsLessEqual(int major, int minor, int build)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}

			if (Minor < minor)
			{
				return true;
			}
			if (Minor > minor)
			{
				return false;
			}
			return Build <= build;
		}

		public bool IsLessEqual(int major, int minor, int build, VersionType type)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}

			if (Minor < minor)
			{
				return true;
			}
			if (Minor > minor)
			{
				return false;
			}

			if (Build < build)
			{
				return true;
			}
			if (Build > build)
			{
				return false;
			}

			return Type <= type;
		}

		public bool IsLessEqual(int major, int minor, int build, VersionType type, int typeNumber)
		{
			if (Major < major)
			{
				return true;
			}
			if (Major > major)
			{
				return false;
			}

			if (Minor < minor)
			{
				return true;
			}
			if (Minor > minor)
			{
				return false;
			}

			if (Build < build)
			{
				return true;
			}
			if (Build > build)
			{
				return false;
			}

			if (Type < type)
			{
				return true;
			}
			if (Type > type)
			{
				return false;
			}

			return TypeNumber <= typeNumber;
		}

		public bool IsLessEqual(string version)
		{
			Version compareVersion = new Version();
			compareVersion.Parse(version);
			return this <= compareVersion;
		}

		public bool IsGreater(int major)
		{
			return Major > major;
		}

		public bool IsGreater(int major, int minor)
		{
			if(Major > major)
			{
				return true;
			}
			if(Major < major)
			{
				return false;
			}
			return Minor > minor;
		}

		public bool IsGreater(int major, int minor, int build)
		{
			if (Major > major)
			{
				return true;
			}
			if (Major < major)
			{
				return false;
			}

			if (Minor > minor)
			{
				return true;
			}
			if (Minor < minor)
			{
				return false;
			}
			return Build > build;
		}

		public bool IsGreater(int major, int minor, int build, VersionType type)
		{
			if (Major > major)
			{
				return true;
			}
			if (Major < major)
			{
				return false;
			}

			if (Minor > minor)
			{
				return true;
			}
			if (Minor < minor)
			{
				return false;
			}

			if (Build > build)
			{
				return true;
			}
			if (Build < build)
			{
				return false;
			}

			return Type > type;
		}

		public bool IsGreater(int major, int minor, int build, VersionType type, int typeNumber)
		{
			if (Major > major)
			{
				return true;
			}
			if (Major < major)
			{
				return false;
			}

			if (Minor > minor)
			{
				return true;
			}
			if (Minor < minor)
			{
				return false;
			}

			if (Build > build)
			{
				return true;
			}
			if (Build < build)
			{
				return false;
			}

			if (Type > type)
			{
				return true;
			}
			if (Type < type)
			{
				return false;
			}

			return TypeNumber > typeNumber;
		}

		public bool IsGreater(string version)
		{
			Version compareVersion = new Version();
			compareVersion.Parse(version);
			return this > compareVersion;
		}

		public bool IsGreaterEqual(int major)
		{
			return Major >= major;
		}

		public bool IsGreaterEqual(int major, int minor)
		{
			if (Major > major)
			{
				return true;
			}
			if (Major < major)
			{
				return false;
			}
			return Minor >= minor;
		}

		public bool IsGreaterEqual(int major, int minor, int build)
		{
			if (Major > major)
			{
				return true;
			}
			if (Major < major)
			{
				return false;
			}

			if (Minor > minor)
			{
				return true;
			}
			if (Minor < minor)
			{
				return false;
			}
			return Build >= build;
		}

		public bool IsGreaterEqual(int major, int minor, int build, VersionType type)
		{
			if (Major > major)
			{
				return true;
			}
			if (Major < major)
			{
				return false;
			}

			if (Minor > minor)
			{
				return true;
			}
			if (Minor < minor)
			{
				return false;
			}

			if (Build > build)
			{
				return true;
			}
			if (Build < build)
			{
				return false;
			}

			return Type >= type;
		}

		public bool IsGreaterEqual(int major, int minor, int build, VersionType type, int typeNumber)
		{
			if (Major > major)
			{
				return true;
			}
			if (Major < major)
			{
				return false;
			}

			if (Minor > minor)
			{
				return true;
			}
			if (Minor < minor)
			{
				return false;
			}

			if (Build > build)
			{
				return true;
			}
			if (Build < build)
			{
				return false;
			}

			if (Type > type)
			{
				return true;
			}
			if (Type < type)
			{
				return false;
			}

			return TypeNumber >= typeNumber;
		}

		public bool IsGreaterEqual(string version)
		{
			Version compareVersion = new Version();
			compareVersion.Parse(version);
			return this >= compareVersion;
		}

		public void Parse(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				throw new Exception($"Invalid version number {version}");
			}

			using (StringReader reader = new StringReader(version))
			{
				string major = string.Empty;
				while(true)
				{
					char c = (char)reader.Read();
					if (c == '.')
					{
						Major = int.Parse(major);
						break;
					}
					major += c;
				}

				string minor = string.Empty;
				while (true)
				{
					int symb = reader.Read();
					if(symb == -1)
					{
						Minor = int.Parse(minor);
						return;
					}

					char c = (char)symb;
					if (c == '.')
					{
						Minor = int.Parse(minor);
						break;
					}
					minor += c;
				}

				string build = string.Empty;
				Type = VersionType.Base;
				TypeNumber = 1;
				while (true)
				{
					int symb = reader.Read();
					if(symb == -1)
					{
						Build = int.Parse(build);
						return;
					}

					char c = (char)symb;
					if (!char.IsDigit(c))
					{
						Build = int.Parse(build);
						switch (c)
						{
							case 'a':
								Type = VersionType.Alpha;
								break;

							case 'b':
								Type = VersionType.Beta;
								break;

							case 'p':
								Type = VersionType.Patch;
								break;

							case 'f':
								Type = VersionType.Final;
								break;

							default:
								throw new Exception($"Unsupported version type {c} for version '{version}'");
						}
						break;
					}
					build += c;
				}

				string typeNumber = string.Empty;
				while (true)
				{
					int symb = reader.Read();
					if (symb == -1)
					{
						TypeNumber = int.Parse(typeNumber);
						return;
					}

					char c = (char)symb;
					if (!char.IsDigit(c))
					{
						TypeNumber = int.Parse(typeNumber);
						break;
					}
					typeNumber += c;
				}
			}
		}

		public int Major { get; private set; }
		public int Minor { get; private set; }
		public int Build { get; private set; }
		public VersionType Type { get; private set; }
		public int TypeNumber { get; private set; }

		public bool IsSet => Major != 0 || Minor != 0 || Build != 0;

		private readonly char[] PartSeparator = new char[] { '.' };
		private readonly char[] ModuleSeparator = new char[] { '\n' };
	}
}
