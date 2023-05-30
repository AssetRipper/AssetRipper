using AssetRipper.SourceGenerated.Subclasses.EnlightenSceneMapping;

namespace AssetRipper.SourceGenerated.Extensions;

public static class EnlightenSceneMappingExtensions
{
	public static bool IsEmpty(this IEnlightenSceneMapping mapping)
	{
		return mapping.Renderers.Count == 0
			&& mapping.SystemAtlases.Count == 0
			&& mapping.Systems.Count == 0
			&& mapping.TerrainChunks.Count == 0
			&& (mapping.Probesets?.Count ?? 0) == 0;
	}
}
