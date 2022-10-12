using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_1001480554;
using AssetRipper.SourceGenerated.Classes.ClassID_1002;
using AssetRipper.SourceGenerated.Classes.ClassID_1029;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Interfaces;
using AssetRipper.SourceGenerated.MarkerInterfaces;
using AssetRipper.SourceGenerated.Subclasses.PPtr_EditorExtension_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_EditorExtensionImpl_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Prefab_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_PrefabInstance_;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;

namespace AssetRipper.Core.Classes
{
	/// <summary>
	/// An asset for creating references to scenes.
	/// </summary>
	public sealed class SceneAsset : UnityObjectBase, ISceneAsset
	{
		public SceneAsset(AssetCollection targetScene, AssetInfo assetInfo) : base(assetInfo)
		{
			TargetScene = targetScene;
		}

		public AssetCollection TargetScene { get; }

		#region ISceneAsset Methods
		IPPtr_EditorExtension_ ISceneAsset.CorrespondingSourceObject_C1032 => throw new NotSupportedException();
		uint ISceneAsset.HideFlags_C1032 { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		bool ISceneAsset.IsWarning_C1032 { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		Utf8String? ISceneAsset.Message_C1032 => throw new NotSupportedException();
		Utf8String ISceneAsset.Name_C1032 => throw new NotSupportedException();
		PPtr_Prefab__2018_3_0_b7? ISceneAsset.PrefabAsset_C1032 => throw new NotSupportedException();
		PPtr_PrefabInstance_? ISceneAsset.PrefabInstance_C1032 => throw new NotSupportedException();
		IPPtr_Prefab_? ISceneAsset.PrefabInternal_C1032 => throw new NotSupportedException();
		HideFlags ISceneAsset.HideFlags_C1032E { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		EditorExtension? ISceneAsset.CorrespondingSourceObject_C1032P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IPrefab? ISceneAsset.PrefabAsset_C1032P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IPrefabInstance? ISceneAsset.PrefabInstance_C1032P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IPrefabMarker? ISceneAsset.PrefabInternal_C1032P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IPPtr_EditorExtension_? IDefaultAsset.CorrespondingSourceObject_C1029 => throw new NotSupportedException();
		PPtr_EditorExtensionImpl_? IDefaultAsset.ExtensionPtr_C1029 => throw new NotSupportedException();
		uint IDefaultAsset.HideFlags_C1029 { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		bool IDefaultAsset.IsWarning_C1029 { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		Utf8String? IDefaultAsset.Message_C1029 => throw new NotSupportedException();
		Utf8String IDefaultAsset.Name_C1029 => throw new NotSupportedException();
		PPtr_Prefab__2018_3_0_b7? IDefaultAsset.PrefabAsset_C1029 => throw new NotSupportedException();
		PPtr_PrefabInstance_? IDefaultAsset.PrefabInstance_C1029 => throw new NotSupportedException();
		IPPtr_Prefab_? IDefaultAsset.PrefabInternal_C1029 => throw new NotSupportedException();
		HideFlags IDefaultAsset.HideFlags_C1029E { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		EditorExtension? IDefaultAsset.CorrespondingSourceObject_C1029P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IEditorExtensionImpl? IDefaultAsset.ExtensionPtr_C1029P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IPrefab? IDefaultAsset.PrefabAsset_C1029P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IPrefabInstance? IDefaultAsset.PrefabInstance_C1029P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		IPrefabMarker? IDefaultAsset.PrefabInternal_C1029P { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		Utf8String IHasName.Name => throw new NotSupportedException();
		string IHasNameString.NameString { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		HideFlags IHasHideFlags.ObjectHideFlags { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		bool IDefaultAsset.Has_CorrespondingSourceObject_C1029() => throw new NotSupportedException();
		bool IDefaultAsset.Has_ExtensionPtr_C1029() => throw new NotSupportedException();
		bool IDefaultAsset.Has_IsWarning_C1029() => throw new NotSupportedException();
		bool ISceneAsset.Has_IsWarning_C1032() => throw new NotSupportedException();
		bool IDefaultAsset.Has_Message_C1029() => throw new NotSupportedException();
		bool ISceneAsset.Has_Message_C1032() => throw new NotSupportedException();
		bool IDefaultAsset.Has_PrefabAsset_C1029() => throw new NotSupportedException();
		bool ISceneAsset.Has_PrefabAsset_C1032() => throw new NotSupportedException();
		bool IDefaultAsset.Has_PrefabInstance_C1029() => throw new NotSupportedException();
		bool ISceneAsset.Has_PrefabInstance_C1032() => throw new NotSupportedException();
		bool IDefaultAsset.Has_PrefabInternal_C1029() => throw new NotSupportedException();
		bool ISceneAsset.Has_PrefabInternal_C1032() => throw new NotSupportedException();
		#endregion
	}
}
