using AssetRipper.Core.IO.Asset;
using System;

namespace AssetRipper.Core.Classes.Renderer
{
	public interface IStaticBatchInfo : IAsset
	{
		ushort FirstSubMesh { get; set; }
		ushort SubMeshCount { get; set; }
	}

	public static class StaticBatchInfoExtensions
	{
		public static void Initialize(this IStaticBatchInfo staticBatchInfo, uint[] subsetIndices)
		{
			if (subsetIndices.Length == 0)
			{
				staticBatchInfo.FirstSubMesh = 0;
				staticBatchInfo.SubMeshCount = 0;
			}
			else
			{
				staticBatchInfo.FirstSubMesh = (ushort)subsetIndices[0];
				staticBatchInfo.SubMeshCount = (ushort)subsetIndices.Length;
				for (int i = 0, j = staticBatchInfo.FirstSubMesh; i < staticBatchInfo.SubMeshCount; i++, j++)
				{
					if (subsetIndices[i] != j)
					{
						throw new Exception("Can't create static batch info from subset indices");
					}
				}
			}
		}

		public static void CopyValues(this IStaticBatchInfo destination, IStaticBatchInfo source)
		{
			destination.FirstSubMesh = source.FirstSubMesh;
			destination.SubMeshCount = source.SubMeshCount;
		}
	}
}
