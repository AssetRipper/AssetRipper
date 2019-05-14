namespace uTinyRipper
{
	public enum TransferMetaFlags
	{
		NoTransferFlags						= 0x0,
		HideInEditorMask					= 0x1,
		NotEditableMask						= 0x10,
		StrongPPtrMask						= 0x40,
		EditorDisplaysCheckBoxMask			= 0x100,
		SimpleEditorMask					= 0x800,
		DebugPropertyMask					= 0x1000,
		AlignBytesFlag						= 0x4000,
		AnyChildUsesAlignBytesFlag			= 0x8000,
		IgnoreWithInspectorUndoMask			= 0x10000,
		IgnoreInMetaFiles					= 0x80000,
		TransferAsArrayEntryNameInMetaFiles	= 0x100000,
		TransferUsingFlowMappingStyle		= 0x200000,
		GenerateBitwiseDifferences			= 0x400000,
		DontAnimate							= 0x800000,
	}
}
