#nullable disable

using AssetRipper.Assets.Metadata;
using System.Collections;

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal abstract class FetchDependenciesEnumerableBase : IEnumerable<(string, PPtr)>, IEnumerator<(string, PPtr)>
{
	private bool _hasBeenUsed;

	protected (string, PPtr) _current;

	private readonly int _initialThreadId;

	protected FetchDependenciesEnumerableBase()
	{
		_initialThreadId = Environment.CurrentManagedThreadId;
	}

	public (string, PPtr) Current => _current;

	object IEnumerator.Current => Current;

	void IDisposable.Dispose()
	{
	}

	public IEnumerator<(string, PPtr)> GetEnumerator()
	{
		FetchDependenciesEnumerableBase result;
		if (!_hasBeenUsed && _initialThreadId == Environment.CurrentManagedThreadId)
		{
			result = this;
		}
		else
		{
			result = CreateNew();
		}
		_hasBeenUsed = true;
		return result;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public abstract bool MoveNext();

	void IEnumerator.Reset() => throw new NotSupportedException();

	private protected abstract FetchDependenciesEnumerableBase CreateNew();
}

#nullable enable
