using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using System.Collections;

namespace AssetRipper.Assets.Generics
{
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
				return list[index].TryGetAsset(file);
			}
			set
			{
				list[index].CopyValues(file.ForceCreatePPtr(value));
			}
		}

		public TPPtr AddNew()
		{
			return list is AccessListBase<TPPtr> accessList
				? accessList.AddNew()
				: throw new NotSupportedException();
		}

		public void Add(TTarget? value)
		{
			if (list is AccessListBase<TPPtr> accessList)
			{
				accessList.AddNew().SetAsset(file, value);
			}
			else
			{
				throw new NotSupportedException();
			}
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
			public override AssetCollection? ResolveCollection(string name) => null;

			private sealed class EmptyAssetCollection : AssetCollection
			{
				public EmptyAssetCollection(Bundle bundle) : base(bundle)
				{
				}

				protected override bool IsCompatibleDependency(AssetCollection dependency) => false;
			}
		}
	}
}
