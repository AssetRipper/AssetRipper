using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Extensions;

internal static class UnityVersionExtensions
{
	public static UnityVersion StripTypeNumber(this UnityVersion version)
	{
		return new UnityVersion(version.Major, version.Minor, version.Build, version.Type);
	}

	public static UnityVersion StripType(this UnityVersion version)
	{
		return new UnityVersion(version.Major, version.Minor, version.Build);
	}

	public static UnityVersion StripBuild(this UnityVersion version)
	{
		return new UnityVersion(version.Major, version.Minor);
	}

	public static UnityVersion StripMinor(this UnityVersion version)
	{
		return new UnityVersion(version.Major);
	}

	public static string ToCleanString(this UnityVersion version, char separator)
	{
		if (version.Type is UnityVersionType.Alpha && version.TypeNumber is 0)
		{
			if (version.Build is 0)
			{
				if (version.Minor is 0)
				{
					return version.Major.ToString();
				}
				else
				{
					return $"{version.Major}{separator}{version.Minor}";
				}
			}
			else
			{
				return $"{version.Major}{separator}{version.Minor}{separator}{version.Build}";
			}
		}
		else
		{
			if (separator == '_')
			{
				return $"{version.Major}_{version.Minor}_{version.Build}_{version.Type.ToCharacter()}{version.TypeNumber}";
			}
			else
			{
				Debug.Assert(separator == '.');
				return $"{version.Major}.{version.Minor}.{version.Build}{version.Type.ToCharacter()}{version.TypeNumber}";
			}
		}
	}
}
