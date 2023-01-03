using AssetRipper.SourceGenerated.Classes.ClassID_205;
using AssetRipper.SourceGenerated.Enums;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class LODGroupExtensions
	{
		public static LODFadeMode GetFadeMode(this ILODGroup group)
		{
			return group.Has_FadeMode_C205()
				? group.FadeMode_C205E
				: group.LODs_C205.FirstOrDefault()?.GetFadeMode() ?? LODFadeMode.None;
		}
	}
}
