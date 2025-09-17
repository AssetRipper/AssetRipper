using AssetRipper.Tpk.Shared;
using AssetRipper.Tpk.TypeTrees;

namespace AssetRipper.AssemblyDumper;

internal sealed class UniversalCommonString
{
	public Dictionary<uint, string> Strings { get; } = new();

	private UniversalCommonString()
	{
	}

	public static UniversalCommonString FromBlob(TpkTypeTreeBlob blob)
	{
		TpkCommonString commonString = blob.CommonString;
		TpkStringBuffer stringBuffer = blob.StringBuffer;

		UniversalCommonString result = new UniversalCommonString();
		result.Strings.EnsureCapacity(commonString.StringBufferIndices.Count);

		int currentKey = 0;
		foreach (ushort stringIndex in commonString.StringBufferIndices)
		{
			string str = stringBuffer[stringIndex];
			result.Strings.Add((uint)currentKey, str);
			currentKey += str.Length + 1;
		}

		return result;
	}
}
