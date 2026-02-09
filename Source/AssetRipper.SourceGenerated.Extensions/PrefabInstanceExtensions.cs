using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.MarkerInterfaces;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PrefabInstanceExtensions
{
	public static string GetName(this IPrefabInstance prefab)
	{
		string? name = prefab.RootGameObjectP?.Name;
		return string.IsNullOrEmpty(name) ? prefab.ClassName : name;
	}

	public static IGameObject GetRootGameObject(this IPrefabInstance prefab)
	{
		return prefab.RootGameObjectP ?? throw new ArgumentException("Prefab has no root GameObject.", nameof(prefab));
	}

	/// <summary>
	/// Sets the <see cref="IPrefabInstance"/> as the internal prefab for all <see cref="IEditorExtension"/>s in the hierarchy.
	/// </summary>
	/// <remarks>
	/// Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
	/// </remarks>
	/// <param name="prefab"></param>
	public static void SetPrefabInternal(this IPrefabInstance prefab)
	{
		if (prefab is IPrefabMarker prefabMarker)
		{
			foreach (IEditorExtension editorExtension in prefab.GetRootGameObject().FetchHierarchy())
			{
				editorExtension.PrefabInternal_C18P = prefabMarker;
			}
		}
	}
}
