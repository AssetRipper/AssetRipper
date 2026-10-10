using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Import.AssetCreation;

/// <summary>
/// The Unity 6.5 GUI style layout, including editor-only scaled backgrounds.
/// </summary>
internal static class GUIStyleTypeTree
{
	public static TypeTreeNodeStruct Create(bool editor)
	{
		List<TypeTreeNodeStruct> fields = [String("m_Name")];
		foreach (string name in new[] { "m_Normal", "m_Hover", "m_Active", "m_Focused", "m_OnNormal", "m_OnHover", "m_OnActive", "m_OnFocused" })
		{
			fields.Add(CreateStyleState(name, editor));
		}
		foreach (string name in new[] { "m_Border", "m_Margin", "m_Padding", "m_Overflow" })
		{
			fields.Add(CreateRectOffset(name));
		}
		fields.Add(Pointer("Font", "m_Font"));
		foreach (string name in new[] { "m_FontSize", "m_FontStyle", "m_Alignment" })
		{
			fields.Add(Scalar("int", name));
		}
		fields.Add(Scalar("bool", "m_WordWrap"));
		fields.Add(Scalar("bool", "m_RichText", true));
		fields.Add(Scalar("int", "m_TextClipping"));
		fields.Add(Scalar("int", "m_ImagePosition"));
		fields.Add(Node("Vector2f", "m_ContentOffset", TransferMetaFlags.TransferUsingFlowMappingStyle, Scalar("float", "x"), Scalar("float", "y")));
		foreach (string name in new[] { "m_ContentSpacing", "m_FixedWidth", "m_FixedHeight" })
		{
			fields.Add(Scalar("float", name));
		}
		foreach (string name in new[] { "m_ImageIsTopAligned", "m_StretchWidth", "m_StretchHeight" })
		{
			fields.Add(Scalar("bool", name));
		}
		fields.Add(Scalar("bool", "m_IsSDF", true));
		return Node("GUIStyle", "Base", TransferMetaFlags.AnyChildUsesAlignBytes, fields.ToArray());
	}

	private static TypeTreeNodeStruct CreateStyleState(string name, bool editor)
	{
		List<TypeTreeNodeStruct> fields = [Pointer("Texture2D", "m_Background")];
		if (editor)
		{
			fields.Add(Node("vector", "m_ScaledBackgrounds", TransferMetaFlags.AnyChildUsesAlignBytes,
				Node("Array", "Array", TransferMetaFlags.AlignBytes, Scalar("int", "size"), Pointer("Texture2D", "data"))));
		}
		fields.Add(Node("ColorRGBA", "m_TextColor", TransferMetaFlags.TransferUsingFlowMappingStyle,
			Scalar("float", "r"), Scalar("float", "g"), Scalar("float", "b"), Scalar("float", "a")));
		return Node("GUIStyleState", name, TransferMetaFlags.AnyChildUsesAlignBytes, fields.ToArray());
	}

	private static TypeTreeNodeStruct CreateRectOffset(string name)
	{
		return Node("RectOffset", name, TransferMetaFlags.NoTransferFlags,
			Scalar("int", "m_Left"), Scalar("int", "m_Right"), Scalar("int", "m_Top"), Scalar("int", "m_Bottom"));
	}

	private static TypeTreeNodeStruct Node(string type, string name, TransferMetaFlags flags, params TypeTreeNodeStruct[] children) => new(type, name, 1, flags, children);
	private static TypeTreeNodeStruct Scalar(string type, string name, bool align = false) => Node(type, name, align ? TransferMetaFlags.AlignBytes : TransferMetaFlags.NoTransferFlags);
	private static TypeTreeNodeStruct Pointer(string type, string name) => Node($"PPtr<{type}>", name, TransferMetaFlags.NoTransferFlags, Scalar("int", "m_FileID"), Scalar("SInt64", "m_PathID"));
	private static TypeTreeNodeStruct String(string name) => Node("string", name, TransferMetaFlags.AlignBytes,
		Node("Array", "Array", TransferMetaFlags.AlignBytes, Scalar("int", "size"), Scalar("char", "data")));
}
