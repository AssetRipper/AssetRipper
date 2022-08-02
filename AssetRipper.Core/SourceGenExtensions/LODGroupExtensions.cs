using AssetRipper.Core.Classes.LODGroup;
using AssetRipper.SourceGenerated.Classes.ClassID_205;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class LODGroupExtensions
	{
		public static LODFadeMode GetFadeMode(this ILODGroup group)
		{
			return group.Has_FadeMode_C205()
				? (LODFadeMode)group.FadeMode_C205
				: group.LODs_C205.FirstOrDefault()?.GetFadeMode() ?? LODFadeMode.None;
		}
	}
}
