using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Extensions;

internal static class EnumHistoryExtensions
{
	public static IEnumerable<KeyValuePair<string, long>> GetFields(this EnumHistory history)
	{
		foreach (EnumMemberHistory member in history.Members.Values)
		{
			if (member.TryGetUniqueValue(out long value, out IEnumerable<KeyValuePair<string, long>>? pairs))
			{
				yield return new KeyValuePair<string, long>(member.Name, value);
			}
			else
			{
				foreach (KeyValuePair<string, long> pair in pairs)
				{
					yield return pair;
				}
			}
		}
	}

	public static IOrderedEnumerable<KeyValuePair<string, long>> GetOrderedFields(this EnumHistory history)
	{
		return history.GetFields().Order(EnumFieldComparer.Instance);
	}

	public static ElementType GetMergedElementType(this EnumHistory history)
	{
		return history.TryGetMergedElementType(out ElementType type) ? type : ElementType.I8;
	}

	private sealed class EnumFieldComparer : IComparer<KeyValuePair<string, long>>
	{
		private EnumFieldComparer() { }

		public static EnumFieldComparer Instance { get; } = new();

		int IComparer<KeyValuePair<string, long>>.Compare(KeyValuePair<string, long> x, KeyValuePair<string, long> y)
		{
			return Compare(x, y);
		}

		/// <summary>
		/// Compare two enum fields
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns>
		/// <paramref name="x"/> &lt; <paramref name="y"/> : -1<br/>
		/// <paramref name="x"/> == <paramref name="y"/> : 0<br/>
		/// <paramref name="x"/> &gt; <paramref name="y"/> : 1<br/>
		/// </returns>
		public static int Compare(KeyValuePair<string, long> x, KeyValuePair<string, long> y)
		{
			if (x.Value != y.Value)
			{
				return x.Value < y.Value ? -1 : 1;
			}
			else
			{
				return x.Key.CompareTo(y.Key);
			}
		}
	}
}
