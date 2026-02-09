using AssetRipper.SourceGenerated.Subclasses.StaticBatchInfo;

namespace AssetRipper.SourceGenerated.Extensions;

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

	public static bool IsDefault(this IStaticBatchInfo staticBatchInfo)
	{
		return staticBatchInfo.FirstSubMesh == 0 && staticBatchInfo.SubMeshCount == 0;
	}
}
