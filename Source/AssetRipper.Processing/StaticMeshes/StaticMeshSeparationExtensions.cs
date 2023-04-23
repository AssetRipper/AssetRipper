using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_23;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using System.Collections;

namespace AssetRipper.Processing.StaticMeshes
{
	internal static class StaticMeshSeparationExtensions
	{
		public static bool IsStaticMeshRenderer(this IUnityObjectBase asset, [NotNullWhen(true)] out IRenderer? renderer)
		{
			renderer = (IRenderer?)(asset as IMeshRenderer) ?? (asset as ISkinnedMeshRenderer);
			return renderer is not null && !renderer.ReferencesDynamicMesh();
		}

		private static bool ReferencesDynamicMesh(this IRenderer renderer)
		{
			return renderer.Has_StaticBatchInfo_C25() && renderer.StaticBatchInfo_C25.SubMeshCount == 0
				|| renderer.Has_SubsetIndices_C25() && renderer.SubsetIndices_C25.Length == 0;
		}

		public static IReadOnlyList<uint> GetSubmeshIndices(this IRenderer renderer)
		{
			return renderer.Has_StaticBatchInfo_C25()
				? new RangeList(renderer.StaticBatchInfo_C25.FirstSubMesh, renderer.StaticBatchInfo_C25.SubMeshCount)
				: renderer.SubsetIndices_C25;
		}

		public static void SetStaticEditorFlagsOnGameObject(this IRenderer renderer)
		{
			//https://github.com/AssetRipper/AssetRipper/issues/702
			IGameObject? gameObject = renderer.GameObject_C25P;
			if (gameObject is not null)
			{
				//When enabling Everything, Unity sets all bits even though it only uses the first 7 bits.
				//In the yaml, this appropriately uint.MaxValue
				//If ContributeGI is disabled, it does not set the reserved bits and displays 126 in the yaml.
				gameObject.StaticEditorFlags_C1 = uint.MaxValue;
			}
			//Should this be done even if the static meshes aren't separated?
		}

		public static void ClearStaticBatchInfo(this IRenderer renderer)
		{
			if (renderer.Has_StaticBatchInfo_C25())
			{
				renderer.StaticBatchInfo_C25.FirstSubMesh = 0;
				renderer.StaticBatchInfo_C25.SubMeshCount = 0;
			}
			else if (renderer.Has_SubsetIndices_C25())
			{
				renderer.SubsetIndices_C25 = Array.Empty<uint>();
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
