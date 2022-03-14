using System;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.Parser
{
	[Flags]
	public enum TransferMetaFlags : uint
	{
		NoTransferFlags = 0x0,
		HideInEditor = 0x1,
		Unknown1 = 0x2,
		Unknown2 = 0x4,
		Unknown3 = 0x8,
		NotEditable = 0x10,
		Unknown5 = 0x20,
		StrongPPtr = 0x40,
		Unknown7 = 0x80,
		TreatIntegerValueAsBoolean = 0x100,
		Unknown9 = 0x200,
		Unknown10 = 0x400,
		SimpleEditor = 0x800,
		DebugProperty = 0x1000,
		Unknown13 = 0x2000,
		AlignBytes = 0x4000,
		AnyChildUsesAlignBytes = 0x8000,
		IgnoreWithInspectorUndo = 0x10000,
		Unknown17 = 0x20000,
		EditorDisplaysCharacterMap = 0x40000,
		IgnoreInMetaFiles = 0x80000,
		TransferAsArrayEntryNameInMetaFiles = 0x100000,
		TransferUsingFlowMappingStyle = 0x200000,
		GenerateBitwiseDifferences = 0x400000,
		DontAnimate = 0x800000,
		TransferHex64 = 0x1000000,
		CharPropertyMask = 0x2000000,
		DontValidateUTF8 = 0x4000000,
		FixedBuffer = 0x8000000,
		DisallowSerializedPropertyModification = 0x10000000,
		Unknown29 = 0x20000000,
		Unknown30 = 0x40000000,
		Unknown31 = 0x80000000,
	}

	public static class TransferMetaFlagsExtensions
	{
		public static bool IsHideInEditor(this TransferMetaFlags _this) => (_this & TransferMetaFlags.HideInEditor) != 0;
		public static bool IsNotEditable(this TransferMetaFlags _this) => (_this & TransferMetaFlags.NotEditable) != 0;
		public static bool IsStrongPPtr(this TransferMetaFlags _this) => (_this & TransferMetaFlags.StrongPPtr) != 0;
		public static bool IsTreatIntegerValueAsBoolean(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TreatIntegerValueAsBoolean) != 0;
		public static bool IsSimpleEditor(this TransferMetaFlags _this) => (_this & TransferMetaFlags.SimpleEditor) != 0;
		public static bool IsDebugProperty(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DebugProperty) != 0;
		public static bool IsAlignBytes(this TransferMetaFlags _this) => (_this & TransferMetaFlags.AlignBytes) != 0;
		public static bool IsAnyChildUsesAlignBytes(this TransferMetaFlags _this) => (_this & TransferMetaFlags.AnyChildUsesAlignBytes) != 0;
		public static bool IsIgnoreWithInspectorUndo(this TransferMetaFlags _this) => (_this & TransferMetaFlags.IgnoreWithInspectorUndo) != 0;
		public static bool IsEditorDisplaysCharacterMap(this TransferMetaFlags _this) => (_this & TransferMetaFlags.EditorDisplaysCharacterMap) != 0;
		public static bool IsIgnoreInMetaFiles(this TransferMetaFlags _this) => (_this & TransferMetaFlags.IgnoreInMetaFiles) != 0;
		public static bool IsTransferAsArrayEntryNameInMetaFiles(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TransferAsArrayEntryNameInMetaFiles) != 0;
		public static bool IsTransferUsingFlowMappingStyle(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TransferUsingFlowMappingStyle) != 0;
		public static bool IsGenerateBitwiseDifferences(this TransferMetaFlags _this) => (_this & TransferMetaFlags.GenerateBitwiseDifferences) != 0;
		public static bool IsDontAnimate(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DontAnimate) != 0;
		public static bool IsTransferHex64(this TransferMetaFlags _this) => (_this & TransferMetaFlags.TransferHex64) != 0;
		public static bool IsCharPropertyMask(this TransferMetaFlags _this) => (_this & TransferMetaFlags.CharPropertyMask) != 0;
		public static bool IsDontValidateUTF8(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DontValidateUTF8) != 0;
		public static bool IsFixedBuffer(this TransferMetaFlags _this) => (_this & TransferMetaFlags.FixedBuffer) != 0;
		public static bool IsDisallowSerializedPropertyModification(this TransferMetaFlags _this) => (_this & TransferMetaFlags.DisallowSerializedPropertyModification) != 0;
	}
}
