using AssetRipper.DocExtraction.DataStructures;
using System.Collections;

namespace AssetRipper.AssemblyDumper.Enums;

internal abstract class EnumDefinitionBase
{
	public abstract string Name { get; }
	public abstract bool MatchesFullName(string fullName);
	public abstract IOrderedEnumerable<KeyValuePair<string, long>> GetOrderedFields();
	public abstract IEnumerable<string> FullNames { get; }
	public abstract bool IsFlagsEnum { get; }
	public abstract ElementType ElementType { get; }
	public abstract UnityVersion MinimumVersion { get; }

	public static Dictionary<string, IReadOnlyList<EnumDefinitionBase>> Create(IEnumerable<EnumHistory> histories)
	{
		Dictionary<string, List<EnumHistory>> dictionary = new();
		foreach (EnumHistory history in histories)
		{
			if (!dictionary.TryGetValue(history.Name, out List<EnumHistory>? list))
			{
				list = new();
				dictionary.Add(history.Name, list);
			}
			list.Add(history);
		}
		Dictionary<string, IReadOnlyList<EnumDefinitionBase>> result = new();
		foreach ((string name, List<EnumHistory> list) in dictionary)
		{
			if (list.Count == 1)
			{
				result.Add(name, new[] { new SingleEnumDefinition(list[0]) });
			}
			else
			{
				List<List<EnumHistory>> boxes = new();
				foreach (EnumHistory history in list)
				{
					List<EnumHistory>? box = boxes.FirstOrDefault(b => HaveSameFields(b[0], history));
					if (box is not null)
					{
						box.Add(history);
					}
					else
					{
						boxes.Add(new() { history });
					}
				}
				List<EnumDefinitionBase> definitions = new(list.Count);
				foreach (List<EnumHistory> box in boxes)
				{
					if (box.Count == 1)
					{
						definitions.Add(new SingleEnumDefinition(box[0]));
					}
					else
					{
						definitions.Add(new MergedEnumDefinition(box));
					}
				}
				result.Add(name, definitions);
			}
		}
		return result;
	}

	private static bool HaveSameFields(EnumHistory x, EnumHistory y)
	{
		return x.GetOrderedFields().SequenceEqual(y.GetOrderedFields());
	}

	private protected sealed class SingleEnumerable<T> : IEnumerable<T>
	{
		public SingleEnumerable(T value)
		{
			Value = value;
		}
		public T Value { get; }
		public IEnumerator<T> GetEnumerator()
		{
			yield return Value;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
