using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.IO
{
	public readonly struct PPtrAccessList<TPPtr, TTarget> : IReadOnlyList<TTarget?> 
		where TPPtr : IPPtr<TTarget>
		where TTarget : IUnityObjectBase
	{
		private static readonly IReadOnlyList<TPPtr> emptyList = Array.Empty<TPPtr>();

		private readonly IReadOnlyList<TPPtr> list;
		private readonly ISerializedFile file;

		public PPtrAccessList(IReadOnlyList<TPPtr> list, ISerializedFile file)
		{
			this.list = list;
			this.file = file;
		}

		public PPtrAccessList(IReadOnlyList<TPPtr> list, IUnityObjectBase asset) : this(list, asset.SerializedFile)
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
				if (value is null)
				{
					list[index].SetNull();
				}
				else
				{
					list[index].CopyValues(file.CreatePPtr(value));
				}
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

		public static PPtrAccessList<TPPtr, TTarget> Empty { get; } = new PPtrAccessList<TPPtr, TTarget>(emptyList, EmptySerializedFile.Shared);
	}
}
