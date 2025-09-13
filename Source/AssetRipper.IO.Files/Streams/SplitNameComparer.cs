namespace AssetRipper.IO.Files.Streams;

internal sealed class SplitNameComparer : IComparer<string>
{
	public static SplitNameComparer Instance { get; } = new();

	public int Compare(string? x, string? y)
	{
		int xNumber = GetSplitIndex(x);
		int yNumber = GetSplitIndex(y);
		return xNumber.CompareTo(yNumber);
	}

	private static int GetSplitIndex(string? value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return -1;
		}

		int i;
		for (i = value.Length - 1; i >= 0; i--)
		{
			if (!char.IsDigit(value[i]))
			{
				i++;
				break;
			}
		}
		string number = value.Substring(i);
		return int.Parse(number);
	}
}
