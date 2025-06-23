using AssetRipper.SourceGenerated.Classes.ClassID_196;
using AssetRipper.SourceGenerated.Classes.ClassID_238;

namespace AssetRipper.SourceGenerated.Extensions;

public static class NavMeshSettingsExtensions
{
	public static void ConvertToEditorFormat(this INavMeshSettings settings)
	{
		INavMeshData? data = settings.NavMeshDataP;
		if (data == null)
		{
			settings.BuildSettings.SetToDefault();
		}
		else
		{
			if (data.Has_NavMeshParams())
			{
				settings.BuildSettings.SetValues(data.NavMeshParams);
			}
			else
			{
				settings.BuildSettings.CopyValues(data.NavMeshBuildSettings);
			}
		}
	}
}
