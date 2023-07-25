using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class PrefabInstanceExtensions
	{
		public static string GetName(this IPrefabInstance prefab)
		{
			string? name = prefab.RootGameObject_C1001P?.Name;
			return string.IsNullOrEmpty(name) ? prefab.ClassName : name;
		}

		public static IGameObject GetRootGameObject(this IPrefabInstance prefab)
		{
			return prefab.RootGameObject_C1001P ?? throw new ArgumentException("Prefab has no root GameObject.", nameof(prefab));
		}
	}
}
