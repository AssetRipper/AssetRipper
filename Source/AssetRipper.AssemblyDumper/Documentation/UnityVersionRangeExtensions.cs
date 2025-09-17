using AssetRipper.Numerics;
using System.Text;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class UnityVersionRangeExtensions
{
	public static IReadOnlyList<UnityVersionRange> GetUnionedRanges(this IEnumerable<UnityVersionRange> ranges)
	{
		List<UnityVersionRange> unionedRanges = new();
		foreach (UnityVersionRange range in ranges)
		{
			if (unionedRanges.Count > 0 && unionedRanges[unionedRanges.Count - 1].CanUnion(range))
			{
				unionedRanges[unionedRanges.Count - 1] = unionedRanges[unionedRanges.Count - 1].MakeUnion(range);
			}
			else
			{
				unionedRanges.Add(range);
			}
		}
		return unionedRanges;
	}

	public static string GetString(this IReadOnlyList<UnityVersionRange> ranges, UnityVersion minimumVersion)
	{
		StringBuilder sb = new();
		sb.AppendUnityVersionRanges(ranges, minimumVersion);
		return sb.ToString();
	}

	public static string GetString(this DiscontinuousRange<UnityVersion> range, UnityVersion minimumVersion)
	{
		StringBuilder sb = new();
		sb.AppendUnityVersionRanges(range, minimumVersion);
		return sb.ToString();
	}
}
