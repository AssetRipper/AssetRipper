using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using System.Collections;
using System.Diagnostics;

namespace AssetRipper.Assets.Generics;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct PPtrAccessList<TPPtr, TTarget> : IReadOnlyList<TTarget?>
	where TPPtr : IPPtr<TTarget>
	where TTarget : IUnityObjectBase
{
	public static PPtrAccessList<TPPtr, TTarget> Empty => new PPtrAccessList<TPPtr, TTarget>(Array.Empty<TPPtr>(), EmptyBundle.Instance.Collection);

	private readonly IReadOnlyList<TPPtr> list;
	private readonly AssetCollection file;

	public PPtrAccessList(IReadOnlyList<TPPtr> list, AssetCollection file)
	{
		this.list = list;
		this.file = file;
	}

	public PPtrAccessList(IReadOnlyList<TPPtr> list, IUnityObjectBase asset) : this(list, asset.Collection)
	{
	}

	public TTarget? this[int index]
	{
		get
		{
			list[index].TryGetAsset(file, out TTarget? result);
			return result;
		}
		set
		{
			list[index].SetAsset(file, value);
		}
	}

	public TPPtr AddNew()
	{
		return GetAccessList().AddNew();
	}

	public void Add(TTarget? item)
	{
		GetAccessList().AddNew().SetAsset(file, item);
	}

	public void AddRange(IEnumerable<TTarget?> items)
	{
		AccessListBase<TPPtr> accessList = GetAccessList();
		foreach (TTarget? value in items)
		{
			accessList.AddNew().SetAsset(file, value);
		}
	}

	public void AddRange<TPPtrSource, TTargetSource>(PPtrAccessList<TPPtrSource, TTargetSource> items)
		where TPPtrSource : IPPtr<TTargetSource>
		where TTargetSource : TTarget
	{
		AccessListBase<TPPtr> accessList = GetAccessList();
		foreach (TTargetSource? value in items)
		{
			accessList.AddNew().SetAsset(file, value);
		}
	}

	private AccessListBase<TPPtr> GetAccessList()
	{
		return list is AccessListBase<TPPtr> accessList
			? accessList
			: throw new NotSupportedException();
	}

	public int Count => list.Count;

	public IEnumerator<TTarget?> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerable<TTarget> WhereNotNull()
	{
		return Count == 0 ? Enumerable.Empty<TTarget>() : InternalNotNull(this);

		static IEnumerable<TTarget> InternalNotNull(PPtrAccessList<TPPtr, TTarget> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				TTarget? asset = list[i];
				if (asset is not null)
				{
					yield return asset;
				}
			}
		}
	}

	private string GetDebuggerDisplay()
	{
		return $"{nameof(Count)} = {Count}";
	}

	private sealed class EmptyBundle : Bundle
	{
		public static EmptyBundle Instance { get; } = new();
		public AssetCollection Collection { get; }
		public override string Name => nameof(EmptyBundle);
		private EmptyBundle()
		{
			Collection = new EmptyAssetCollection(this);
		}

		protected override bool IsCompatibleBundle(Bundle bundle) => false;
		protected override bool IsCompatibleCollection(AssetCollection collection) => collection is EmptyAssetCollection;

		private sealed class EmptyAssetCollection : AssetCollection
		{
			public EmptyAssetCollection(Bundle bundle) : base(bundle)
			{
			}

			protected override bool IsCompatibleDependency(AssetCollection dependency) => false;
		}
	}
}
