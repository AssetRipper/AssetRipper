using System.Collections;

namespace AssetRipper.Import.Configuration;

public abstract class DataSet : DataEntry, IEnumerable
{
	public StringAccessor Strings => this;
	public abstract void RemoveAt(int index);
	public abstract int Count { get; }
	protected abstract string GetAsString(int index);
	protected abstract void SetFromString(int index, string value);
	protected abstract void AddString(string value);
	protected abstract void AddStrings(IEnumerable<string> values);
	public abstract IEnumerator GetEnumerator();

	public readonly record struct StringAccessor(DataSet Values) : IReadOnlyList<string>
	{
		public string this[int index]
		{
			get => Values.GetAsString(index);
			set => Values.SetFromString(index, value);
		}

		public int Count => Values.Count;

		public void Add(string item) => Values.AddString(item);

		public void AddRange(IEnumerable<string> collection) => Values.AddStrings(collection);

		public void Clear() => Values.Clear();

		public void RemoveAt(int index) => Values.RemoveAt(index);

		public IEnumerator<string> GetEnumerator()
		{
			for (int i = 0; i < Values.Count; i++)
			{
				yield return Values.GetAsString(i);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public static implicit operator StringAccessor(DataSet values) => new(values);
		public static implicit operator DataSet(StringAccessor accessor) => accessor.Values;
	}
}
public class DataSet<T> : DataSet, IReadOnlyList<T>
{
	private readonly List<T> list = new();
	private readonly DataSerializer<T> serializer;

	public DataSet(DataSerializer<T> serializer)
	{
		this.serializer = serializer;
		list = new();
	}

	public DataSet(DataSerializer<T> serializer, List<T> list)
	{
		this.serializer = serializer;
		this.list = list;
	}

	public sealed override int Count => list.Count;

	public T this[int index]
	{
		get => list[index];
		set => list[index] = value;
	}

	public void Add(T item) => list.Add(item);

	public void AddNew() => Add(serializer.CreateNew());

	protected sealed override void AddString(string value) => Add(serializer.Deserialize(value));

	protected sealed override void AddStrings(IEnumerable<string> values)
	{
		EnsureCapacityIfCountAvailable(values);
		foreach (string value in values)
		{
			AddString(value);
		}

		void EnsureCapacityIfCountAvailable(IEnumerable<string> values)
		{
			if (values is IReadOnlyCollection<string> collection)
			{
				list.EnsureCapacity(list.Count + collection.Count);
			}
		}
	}

	public sealed override void Clear() => list.Clear();

	public bool Contains(T item) => list.Contains(item);

	public sealed override void RemoveAt(int index) => list.RemoveAt(index);

	protected sealed override string GetAsString(int index) => serializer.Serialize(this[index]);

	protected sealed override void SetFromString(int index, string value) => this[index] = serializer.Deserialize(value);

	public override IEnumerator<T> GetEnumerator() => list.GetEnumerator();
}
