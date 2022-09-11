using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_196;
using AssetRipper.SourceGenerated.Classes.ClassID_238;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class NavMeshSettingsExtensions
	{
		public static void ConvertToEditorFormat(this INavMeshSettings settings)
		{
			INavMeshData? data = settings.NavMeshData_C196?.TryGetAsset(settings.SerializedFile);
			if (data == null)
			{
				settings.BuildSettings_C196.SetToDefault();
			}
			else
			{
				if (data.Has_NavMeshParams_C238())
				{
					settings.BuildSettings_C196.SetValues(data.NavMeshParams_C238);
				}
				else
				{
					//settings.BuildSettings_C196.CopyValues(data.NavMeshBuildSettings_C238);
					settings.BuildSettings_C196.SetToDefault();
				}
			}
		}
	}
}
