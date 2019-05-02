using System;

namespace uTinyRipper
{
	[Flags]
	public enum TransferInstructionFlags : uint
	{
		NoTransferInstructionFlags				= 0x0,
		NeedsInstanceIDRemapping				= 0x1,
		AssetMetaDataOnly						= 0x2,
		YamlGlobalPPtrReference					= 0x4,
		LoadAndUnloadAssetsDuringBuild			= 0x8,
		SerializeDebugProperties				= 0x10,
		IgnoreDebugPropertiesForIndex			= 0x20,
		BuildPlayerOnlySerializeBuildProperties	= 0x40,
		Workaround35MeshSerializationFuckup		= 0x80,
		/// <summary>
		/// Has this file been built for release game
		/// </summary>
		SerializeGameRelease					= 0x100,
		SwapEndianess							= 0x200,
		SaveGlobalManagers						= 0x400,
		DontReadObjectsFromDiskBeforeWriting	= 0x800,
		SerializeMonoReload						= 0x1000,
		DontRequireAllMetaFlags					= 0x2000,
		/// <summary>
		/// Is prefab's format read
		/// </summary>
		SerializeForPrefabSystem				= 0x4000,
		WarnAboutLeakedObjects					= 0x8000,
		EditorPlayMode							= 0x40000,
		BuildResourceImage						= 0x80000,
		SerializeEditorMinimalScene				= 0x200000,
		GenerateBakedPhysixMeshes				= 0x400000,
		ThreadedSerialization					= 0x800000,
		IsBuiltinResourcesFile					= 0x1000000,
		PerformUnloadDependencyTracking			= 0x2000000,
		DisableWriteTypeTree					= 0x4000000,
		AutoreplaceEditorWindow					= 0x8000000,
		DontCreateMonoBehaviourScriptWrapper	= 0x10000000,
		SerializeForInspector					= 0x20000000,
		SerializedAssetBundleVersion			= 0x40000000,
		AllowTextSerialization					= 0x80000000,
	}

	public static class TransferInstructionFlagsExtensions
	{
		public static bool IsDebug(this TransferInstructionFlags _this)
		{
			return (_this & TransferInstructionFlags.SerializeDebugProperties) != 0;
		}
		public static bool IsRelease(this TransferInstructionFlags _this)
		{
			return (_this & TransferInstructionFlags.SerializeGameRelease) != 0;
		}
		public static bool IsForPrefab(this TransferInstructionFlags _this)
		{
			return (_this & TransferInstructionFlags.SerializeForPrefabSystem) != 0;
		}
		public static bool IsBuiltinResources(this TransferInstructionFlags _this)
		{
			return (_this & TransferInstructionFlags.IsBuiltinResourcesFile) != 0;
		}
	}
}
