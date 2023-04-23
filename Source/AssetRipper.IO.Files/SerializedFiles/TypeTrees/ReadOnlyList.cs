using System.Collections;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

internal sealed class ReadOnlyList<TSource, TOut> : IReadOnlyList<TOut> where TSource : TOut
{
	public ReadOnlyList(IReadOnlyList<TSource> source)
	{
		this.Source = source ?? throw new ArgumentNullException(nameof(source));
	}

	public TOut this[int index] => Source[index];

	public int Count => Source.Count;

	public IReadOnlyList<TSource> Source { get; }

	public IEnumerator<TOut> GetEnumerator()
	{
		foreach (TSource sourceElement in Source)
		{
			yield return sourceElement;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
