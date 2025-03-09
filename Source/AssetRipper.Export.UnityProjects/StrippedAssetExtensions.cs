using AssetRipper.Assets;
using AssetRipper.Processing.Prefabs;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.Yaml;

namespace AssetRipper.Export.UnityProjects;

public static class StrippedAssetExtensions
{
	public static bool IsStripped(this IUnityObjectBase asset)
	{
		if (asset.MainAsset is GameObjectHierarchyObject hierarchy)
		{
			return hierarchy.StrippedAssets.Contains(asset);
		}
		return false;
	}

	internal static void RemoveStrippedFields(this IUnityObjectBase asset, YamlMappingNode root)
	{
		RemoveStrippedFields(root, asset is IMonoBehaviour ? AllowedMonoBehaviourFields : AllowedAssetFields);
	}

	private static void RemoveStrippedFields(YamlMappingNode root, HashSet<string> allowedFields)
	{
		for (int i = root.Children.Count - 1; i >= 0; i--)
		{
			YamlNode child = root.Children[i].Key;
			if (child is not YamlScalarNode scalar || !allowedFields.Contains(scalar.Value))
			{
				root.Children.RemoveAt(i);
			}
		}
	}

	private static HashSet<string> AllowedAssetFields { get; } =
	[
		"m_CorrespondingSourceObject",
		"m_PrefabAsset",
		"m_PrefabInstance",
		"m_PrefabInternal",
		"m_PrefabParentObject",
	];

	private static HashSet<string> AllowedMonoBehaviourFields { get; } =
	[
		"m_CorrespondingSourceObject",
		"m_EditorClassIdentifier",
		"m_EditorHideFlags",
		"m_Enabled",
		"m_GameObject",
		"m_Name",
		"m_PrefabAsset",
		"m_PrefabInstance",
		"m_PrefabInternal",
		"m_PrefabParentObject",
		"m_Script",
	];
}
