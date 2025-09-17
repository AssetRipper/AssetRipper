namespace AssetRipper.AssemblyDumper.Documentation;

internal static class UnityVersionRangeUtils
{
	public static string GetUnityVersionRangeString(UnityVersionRange range)
	{
		string start = range.Start == UnityVersion.MinVersion || range.Start == SharedState.Instance.MinVersion
			? "Min"
			: range.Start.ToCleanString('.');
		string end = range.End == UnityVersion.MaxVersion
			? "Max"
			: range.End.ToCleanString('.');
		return $"{start} to {end}";
	}
}
