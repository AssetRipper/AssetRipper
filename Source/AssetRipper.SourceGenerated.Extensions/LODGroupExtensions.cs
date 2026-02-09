using AssetRipper.SourceGenerated.Classes.ClassID_205;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LODGroupExtensions
{
	public static LODFadeMode GetFadeMode(this ILODGroup group)
	{
		return group.Has_FadeMode()
			? group.FadeModeE
			: group.LODs.FirstOrDefault()?.GetFadeMode() ?? LODFadeMode.None;
	}
}
