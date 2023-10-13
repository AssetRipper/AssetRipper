using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Extensions;
using System.Collections;

namespace AssetRipper.Processing.StaticMeshes
{
	internal static class StaticMeshSeparationExtensions
	{
		public static bool IsStaticMeshRenderer(this IUnityObjectBase asset, [NotNullWhen(true)] out IRenderer? renderer)
		{
			renderer = asset as IRenderer;
			return renderer is not null && !renderer.ReferencesDynamicMesh();
		}

		private static bool ReferencesDynamicMesh(this IRenderer renderer)
		{
			return renderer.Has_StaticBatchInfo_C25() && renderer.StaticBatchInfo_C25.IsDefault()
				|| renderer.Has_SubsetIndices_C25() && renderer.SubsetIndices_C25.Count == 0;
		}

		public static IReadOnlyList<uint> GetSubmeshIndices(this IRenderer renderer)
		{
			return renderer.Has_StaticBatchInfo_C25()
				? new RangeList(renderer.StaticBatchInfo_C25.FirstSubMesh, renderer.StaticBatchInfo_C25.SubMeshCount)
				: renderer.SubsetIndices_C25;
		}

		public static void ClearStaticBatchInfo(this IRenderer renderer)
		{
			if (renderer.Has_StaticBatchInfo_C25())
			{
				renderer.StaticBatchInfo_C25.Reset();
			}
			else if (renderer.Has_SubsetIndices_C25())
			{
				renderer.SubsetIndices_C25.Clear();
			}
			renderer.StaticBatchRoot_C25P = null;
		}

		private sealed class RangeList : IReadOnlyList<uint>
		{
			private readonly uint start;
			private readonly uint count;

			public RangeList(uint start, uint count)
			{
				this.start = start;
				this.count = count;
			}

			public uint this[int index]
			{
				get
				{
					if (index < 0 || index >= count)
					{
						throw new ArgumentOutOfRangeException(nameof(index));
					}
					else
					{
						return start + (uint)index;
					}
				}
			}

			public int Count => (int)count;

			public IEnumerator<uint> GetEnumerator()
			{
				for (uint i = 0; i < count; i++)
				{
					yield return start + i;
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
