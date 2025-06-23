namespace AssetRipper.IO.Files.SerializedFiles;

[Flags]
public enum TransferInstructionFlags : uint
{
	NoTransferInstructionFlags = 0x0,
	/// <summary>
	/// Should we convert PPtrs into pathID, fileID using the PerisistentManager or should we just store the memory InstanceID in the fileID?
	/// </summary>
	/// <remarks>
	/// Also called ReadWriteFromSerializedFile
	/// </remarks>
	NeedsInstanceIDRemapping = 0x1,
	/// <summary>
	/// Only serialize data needed for .meta files
	/// </summary>
	AssetMetaDataOnly = 0x2,
	/// <summary>
	/// Also called HandleDrivenProperties
	/// </summary>
	YamlGlobalPPtrReference = 0x4,
	LoadAndUnloadAssetsDuringBuild = 0x8,
	/// <summary>
	/// Should we serialize debug properties (eg. Serialize mono private variables)?
	/// </summary>
	SerializeDebugProperties = 0x10,
	/// <summary>
	/// Should we ignore Debug properties when calculating the TypeTree index?
	/// </summary>
	IgnoreDebugPropertiesForIndex = 0x20,
	/// <summary>
	/// Used by the build player to make materials cull any properties that aren't used anymore.
	/// </summary>
	BuildPlayerOnlySerializeBuildProperties = 0x40,
	/// <summary>
	/// Also called IsCloningObject
	/// </summary>
	Workaround35MeshSerializationFuckup = 0x80,
	/// <summary>
	/// Is this a game or a project file?
	/// </summary>
	SerializeGameRelease = 0x100,
	/// <summary>
	/// Should we swap endianess when reading / writing a file?
	/// </summary>
	SwapEndianess = 0x200,
	/// <summary>
	/// Should global managers be saved when writing the game build?
	/// </summary>
	/// <remarks>
	/// Also called ResolveStreamedResourceSources
	/// </remarks>
	SaveGlobalManagers = 0x400,
	DontReadObjectsFromDiskBeforeWriting = 0x800,
	/// <summary>
	/// Should we backup mono mono variables for an assembly reload?
	/// </summary>
	SerializeMonoReload = 0x1000,
	/// <summary>
	/// Can Unity fast path calculating all meta data? This lets it skip a bunch of code when serializing mono data.
	/// </summary>
	DontRequireAllMetaFlags = 0x2000,
	SerializeForPrefabSystem = 0x4000,
	/// <summary>
	/// Also called SerializeForSlimPlayer
	/// </summary>
	WarnAboutLeakedObjects = 0x8000,
	LoadPrefabAsScene = 0x10000,
	SerializeCopyPasteTransfer = 0x20000,
	/// <summary>
	/// Also called SkipSerializeToTempFile
	/// </summary>
	EditorPlayMode = 0x40000,
	BuildResourceImage = 0x80000,
	DontWriteUnityVersion = 0x100000,
	/// <summary>
	/// Binary scene files in the Editor.
	/// </summary>
	/// <remarks>
	/// Causes PrefabInstance.RootGameObject to not be included in type trees.
	/// Prefab.RootGameObject is unaffected, ie this flag only has an impact on 2018.3+.
	/// </remarks>
	SerializeEditorMinimalScene = 0x200000,
	GenerateBakedPhysixMeshes = 0x400000,
	ThreadedSerialization = 0x800000,
	IsBuiltinResourcesFile = 0x1000000,
	PerformUnloadDependencyTracking = 0x2000000,
	DisableWriteTypeTree = 0x4000000,
	AutoreplaceEditorWindow = 0x8000000,
	DontCreateMonoBehaviourScriptWrapper = 0x10000000,
	SerializeForInspector = 0x20000000,
	/// <summary>
	/// When writing with typetrees disabled, allow later Unity versions an attempt to read SerializedFile.
	/// </summary>
	SerializedAssetBundleVersion = 0x40000000,
	AllowTextSerialization = 0x80000000,
}

public static class TransferInstructionFlagsExtensions
{
	public static bool IsRelease(this TransferInstructionFlags _this)
	{
		return (_this & TransferInstructionFlags.SerializeGameRelease) != 0;
	}
	public static bool IsForPrefab(this TransferInstructionFlags _this)
	{
		return (_this & TransferInstructionFlags.SerializeForPrefabSystem) != 0;
	}
	public static bool IsEditorScene(this TransferInstructionFlags _this)
	{
		return (_this & TransferInstructionFlags.SerializeEditorMinimalScene) != 0;
	}
	public static bool IsBuiltinResources(this TransferInstructionFlags _this)
	{
		return (_this & TransferInstructionFlags.IsBuiltinResourcesFile) != 0;
	}
}
