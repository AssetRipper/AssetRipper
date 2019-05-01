using System;

namespace uTinyRipper.Classes.Objects
{
	/// <summary>
	/// Bit mask that controls object destruction, saving and visibility in inspectors.
	/// </summary>
	[Flags]
	public enum HideFlags
	{
		/// <summary>
		/// A normal, visible object. This is the default.
		/// </summary>
		None					= 0,
		/// <summary>
		/// The object will not appear in the hierarchy.
		/// </summary>
		HideInHierarchy			= 1,
		/// <summary>
		/// It is not possible to view it in the inspector.
		/// </summary>
		HideInInspector			= 2,
		/// <summary>
		/// The object will not be saved to the Scene in the editor.
		/// </summary>
		DontSaveInEditor		= 4,
		/// <summary>
		/// The object is not be editable in the inspector.
		/// </summary>
		NotEditable				= 8,
		/// <summary>
		/// The object will not be saved when building a player.
		/// </summary>
		DontSaveInBuild			= 16,
		/// <summary>
		/// The object will not be unloaded by Resources.UnloadUnusedAssets.
		/// </summary>
		DontUnloadUnusedAsset	= 32,
		/// <summary>
		/// The object will not be saved to the Scene. It will not be destroyed when a new Scene is loaded.
		/// It is a shortcut for HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset.
		/// </summary>
		DontSave				= 52,
		/// <summary>
		/// The GameObject is not shown in the Hierarchy, not saved to to Scenes, and not unloaded by Resources.UnloadUnusedAssets.
		/// </summary>
		HideAndDontSave			= 61
	}
}
