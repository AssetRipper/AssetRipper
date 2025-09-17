using AssetRipper.Numerics;
using System.Text;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class StringBuilderExtensions
{
	public static void AppendLineAndThreeTabs(this StringBuilder sb) => sb.Append("\n\t\t\t");
	public static void AppendBreakTag(this StringBuilder sb) => sb.Append("<br />");
	public static void AppendUnityVersionRange(this StringBuilder sb, Range<UnityVersion> range, UnityVersion minimumVersion)
	{
		sb.Append(range.Start == minimumVersion ? "Min" : range.Start.ToCleanString('.'));
		sb.Append(" to ");
		sb.Append(range.End == UnityVersion.MaxVersion ? "Max" : range.End.ToCleanString('.'));
	}
	public static void AppendUnityVersionRanges(this StringBuilder sb, IReadOnlyList<UnityVersionRange> ranges, UnityVersion minimumVersion)
	{
		sb.AppendUnityVersionRange(ranges[0], minimumVersion);
		for (int i = 1; i < ranges.Count; i++)
		{
			sb.Append(", ");
			sb.AppendUnityVersionRange(ranges[i], minimumVersion);
		}
	}
	public static void AppendUnityVersionRanges(this StringBuilder sb, DiscontinuousRange<UnityVersion> range, UnityVersion minimumVersion)
	{
		if (!range.IsEmpty())
		{
			sb.AppendUnityVersionRange(range[0], minimumVersion);
			for (int i = 1; i < range.Count; i++)
			{
				sb.Append(", ");
				sb.AppendUnityVersionRange(range[i], minimumVersion);
			}
		}
	}
}
