using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.ResourceFiles;
using System.Collections;

namespace AssetRipper.Assets.Generics
{
	public readonly struct PPtrAccessList<TPPtr, TTarget> : IReadOnlyList<TTarget?>
		where TPPtr : IPPtr<TTarget>
		where TTarget : IUnityObjectBase
	{
		private static readonly IReadOnlyList<TPPtr> emptyList = Array.Empty<TPPtr>();
		private static readonly EmptyAssetCollection emptyCollection = new EmptyBundle().Collection;

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

		public int Count => list.Count;

		public IEnumerator<TTarget?> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public static PPtrAccessList<TPPtr, TTarget> Empty => new PPtrAccessList<TPPtr, TTarget>(emptyList, emptyCollection);

		private sealed class EmptyAssetCollection : AssetCollection
		{
			public EmptyAssetCollection(Bundle bundle) : base(bundle)
			{
			}
		}

		private sealed class EmptyBundle : Bundle
		{
			public EmptyAssetCollection Collection { get; }
			public override string Name => nameof(EmptyBundle);
			public EmptyBundle()
			{
				Collection = new EmptyAssetCollection(this);
			}

			protected override bool IsCompatibleBundle(Bundle bundle) => false;
			protected override bool IsCompatibleCollection(AssetCollection collection) => collection is EmptyAssetCollection;
			public override AssetCollection? ResolveCollection(string name) => null;
			protected override ResourceFile? ResolveResourceInternal(string originalName, string fixedName) => null;
		}
	}
}
