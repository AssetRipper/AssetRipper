namespace AssetRipper.IO.Files.SerializedFiles;

[Flags]
public enum TransferMetaFlags : uint
{
	NoTransferFlags = 0x0,
	/// <summary>
	/// Putting this mask in a transfer will make the variable be hidden in the property editor.
	/// </summary>
	HideInEditor = 0x1,
	Unknown1 = 0x2,
	Unknown2 = 0x4,
	Unknown3 = 0x8,
	/// <summary>
	/// Makes a variable not editable in the property editor.
	/// </summary>
	NotEditable = 0x10,
	Unknown5 = 0x20,
	/// <summary>
	/// There are 3 types of PPtrs: <see cref="StrongPPtr"/>, default (weak pointer)
	/// a Strong PPtr forces the referenced object to be cloned.
	/// A Weak PPtr doesnt clone the referenced object, but if the referenced object is being cloned anyway (eg. If another (strong) pptr references this object)
	/// this PPtr will be remapped to the cloned object.
	/// If an object referenced by a WeakPPtr is not cloned, it will stay the same when duplicating and cloning, but be NULLed when templating.
	/// </summary>
	StrongPPtr = 0x40,
	Unknown7 = 0x80,
	/// <summary>
	/// Makes an integer variable appear as a checkbox in the editor.
	/// </summary>
	TreatIntegerValueAsBoolean = 0x100,
	Unknown9 = 0x200,
	Unknown10 = 0x400,
	/// <summary>
	/// Show in simplified editor
	/// </summary>
	SimpleEditor = 0x800,
	/// <summary>
	/// For when the options of a serializer tell you to serialize debug properties (<see cref="TransferInstructionFlags.SerializeDebugProperties"/>).
	/// All debug properties have to be marked <see cref="DebugProperty"/>.
	/// Debug properties are shown in expert mode in the inspector but are not serialized normally.
	/// </summary>
	DebugProperty = 0x1000,
	Unknown13 = 0x2000,
	AlignBytes = 0x4000,
	AnyChildUsesAlignBytes = 0x8000,
	IgnoreWithInspectorUndo = 0x10000,
	Unknown17 = 0x20000,
	EditorDisplaysCharacterMap = 0x40000,
	/// <summary>
	/// Ignore this property when reading or writing .meta files
	/// </summary>
	IgnoreInMetaFiles = 0x80000,
	/// <summary>
	/// When reading meta files and this property is not present, read array entry name instead (for backwards compatibility).
	/// </summary>
	TransferAsArrayEntryNameInMetaFiles = 0x100000,
	/// <summary>
	/// When writing YAML Files, uses the flow mapping style (all properties in one line, with "{}").
	/// </summary>
	TransferUsingFlowMappingStyle = 0x200000,
	/// <summary>
	/// Tells SerializedProperty to generate bitwise difference information for this field.
	/// </summary>
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
	public static IEnumerable<string> Split(this TransferMetaFlags flags)
	{
		if (flags == TransferMetaFlags.NoTransferFlags)
		{
			yield return nameof(TransferMetaFlags.NoTransferFlags);
		}
		else
		{
			if (flags.HasFlag(TransferMetaFlags.HideInEditor))
			{
				yield return nameof(TransferMetaFlags.HideInEditor);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown1))
			{
				yield return nameof(TransferMetaFlags.Unknown1);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown2))
			{
				yield return nameof(TransferMetaFlags.Unknown2);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown3))
			{
				yield return nameof(TransferMetaFlags.Unknown3);
			}

			if (flags.HasFlag(TransferMetaFlags.NotEditable))
			{
				yield return nameof(TransferMetaFlags.NotEditable);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown5))
			{
				yield return nameof(TransferMetaFlags.Unknown5);
			}

			if (flags.HasFlag(TransferMetaFlags.StrongPPtr))
			{
				yield return nameof(TransferMetaFlags.StrongPPtr);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown7))
			{
				yield return nameof(TransferMetaFlags.Unknown7);
			}

			if (flags.HasFlag(TransferMetaFlags.TreatIntegerValueAsBoolean))
			{
				yield return nameof(TransferMetaFlags.TreatIntegerValueAsBoolean);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown9))
			{
				yield return nameof(TransferMetaFlags.Unknown9);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown10))
			{
				yield return nameof(TransferMetaFlags.Unknown10);
			}

			if (flags.HasFlag(TransferMetaFlags.SimpleEditor))
			{
				yield return nameof(TransferMetaFlags.SimpleEditor);
			}

			if (flags.HasFlag(TransferMetaFlags.DebugProperty))
			{
				yield return nameof(TransferMetaFlags.DebugProperty);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown13))
			{
				yield return nameof(TransferMetaFlags.Unknown13);
			}

			if (flags.HasFlag(TransferMetaFlags.AlignBytes))
			{
				yield return nameof(TransferMetaFlags.AlignBytes);
			}

			if (flags.HasFlag(TransferMetaFlags.AnyChildUsesAlignBytes))
			{
				yield return nameof(TransferMetaFlags.AnyChildUsesAlignBytes);
			}

			if (flags.HasFlag(TransferMetaFlags.IgnoreWithInspectorUndo))
			{
				yield return nameof(TransferMetaFlags.IgnoreWithInspectorUndo);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown17))
			{
				yield return nameof(TransferMetaFlags.Unknown17);
			}

			if (flags.HasFlag(TransferMetaFlags.EditorDisplaysCharacterMap))
			{
				yield return nameof(TransferMetaFlags.EditorDisplaysCharacterMap);
			}

			if (flags.HasFlag(TransferMetaFlags.IgnoreInMetaFiles))
			{
				yield return nameof(TransferMetaFlags.IgnoreInMetaFiles);
			}

			if (flags.HasFlag(TransferMetaFlags.TransferAsArrayEntryNameInMetaFiles))
			{
				yield return nameof(TransferMetaFlags.TransferAsArrayEntryNameInMetaFiles);
			}

			if (flags.HasFlag(TransferMetaFlags.TransferUsingFlowMappingStyle))
			{
				yield return nameof(TransferMetaFlags.TransferUsingFlowMappingStyle);
			}

			if (flags.HasFlag(TransferMetaFlags.GenerateBitwiseDifferences))
			{
				yield return nameof(TransferMetaFlags.GenerateBitwiseDifferences);
			}

			if (flags.HasFlag(TransferMetaFlags.DontAnimate))
			{
				yield return nameof(TransferMetaFlags.DontAnimate);
			}

			if (flags.HasFlag(TransferMetaFlags.TransferHex64))
			{
				yield return nameof(TransferMetaFlags.TransferHex64);
			}

			if (flags.HasFlag(TransferMetaFlags.CharPropertyMask))
			{
				yield return nameof(TransferMetaFlags.CharPropertyMask);
			}

			if (flags.HasFlag(TransferMetaFlags.DontValidateUTF8))
			{
				yield return nameof(TransferMetaFlags.DontValidateUTF8);
			}

			if (flags.HasFlag(TransferMetaFlags.FixedBuffer))
			{
				yield return nameof(TransferMetaFlags.FixedBuffer);
			}

			if (flags.HasFlag(TransferMetaFlags.DisallowSerializedPropertyModification))
			{
				yield return nameof(TransferMetaFlags.DisallowSerializedPropertyModification);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown29))
			{
				yield return nameof(TransferMetaFlags.Unknown29);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown30))
			{
				yield return nameof(TransferMetaFlags.Unknown30);
			}

			if (flags.HasFlag(TransferMetaFlags.Unknown31))
			{
				yield return nameof(TransferMetaFlags.Unknown31);
			}
		}
	}
}
