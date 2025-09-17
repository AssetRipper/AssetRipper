using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Extensions;

internal static class EnumMemberHistoryExtensions
{
	public static IEnumerable<KeyValuePair<string, long>> GetFields(this EnumMemberHistory member)
	{
		if (IsUnique(member.Value, out long uniqueValue))
		{
			yield return new KeyValuePair<string, long>(member.Name, uniqueValue);
		}
		else
		{
			foreach (long value in member.Value.Values.Where(v => v != -1).ToHashSet())
			{
				string fieldName = GetEnumFieldName(member.Name, value);
				yield return new KeyValuePair<string, long>(fieldName, value);
			}
		}
	}

	public static bool TryGetUniqueValue(
		this EnumMemberHistory member,
		out long value,
		[NotNullWhen(false)] out IEnumerable<KeyValuePair<string, long>>? fields)
	{
		if (IsUnique(member.Value, out long uniqueValue))
		{
			value = uniqueValue;
			fields = null;
			return true;
		}
		else
		{
			value = default;
			fields = ToEnumerable(member.Name, member.Value.Values.ToHashSet());
			return false;
		}

		static IEnumerable<KeyValuePair<string, long>> ToEnumerable(string memberName, HashSet<long> values)
		{
			foreach (long value in values.Where(v => v != -1))
			{
				string fieldName = GetEnumFieldName(memberName, value);
				yield return new KeyValuePair<string, long>(fieldName, value);
			}
		}
	}

	private static bool IsUnique(VersionedList<long> list, out long value)
	{
		if (list.Count is 1)
		{
			value = list[0].Value;
			return true;
		}
		else if (list.Count == 2 && list[1].Value is -1)
		{
			//This member was made obsolete and given a value of -1.
			value = list[0].Value;
			return true;
		}
		else if (list.Count == 2 && list[0].Value == unchecked(-list[1].Value))
		{
			//This member was made obsolete and given a value of -1 * its original value.
			value = list[0].Value;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}

	private static string GetEnumFieldName(string name, long value)
	{
		return value switch
		{
			long.MinValue => $"{name}_MinS64",
			long.MaxValue => $"{name}_MaxS64",
			int.MinValue => $"{name}_MinS32",
			int.MaxValue => $"{name}_MaxS32",
			short.MinValue => $"{name}_MinS16",
			short.MaxValue => $"{name}_MaxS16",
			sbyte.MinValue => $"{name}_MinS8",
			sbyte.MaxValue => $"{name}_MaxS8",
			uint.MaxValue => $"{name}_MaxU32",
			ushort.MaxValue => $"{name}_MaxU16",
			byte.MaxValue => $"{name}_MaxU8",
			< 0 => $"{name}_N{-value}",
			_ => $"{name}_{value}",
		};
	}
}
