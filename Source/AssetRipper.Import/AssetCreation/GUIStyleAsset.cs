using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Import.AssetCreation;

/// <summary>
/// Unity 6.5 GUI styles add content spacing and the SDF flag.
/// Scaled backgrounds are serialized only in the editor.
/// </summary>
internal sealed class GUIStyleAsset : UnityAssetBase
{
	private readonly UnityVersion version;
	private readonly SerializableStructure releaseFields;
	private readonly SerializableStructure editorFields;

	public GUIStyleAsset(UnityVersion version)
	{
		this.version = version;
		releaseFields = SerializableTreeType.FromRootNode(MakeTree(false)).CreateSerializableStructure();
		editorFields = SerializableTreeType.FromRootNode(MakeTree(true)).CreateSerializableStructure();
		releaseFields.InitializeFields(version);
		editorFields.InitializeFields(version);
	}

	public override void ReadRelease(ref EndianSpanReader reader)
	{
		releaseFields.Read(ref reader, version, TransferInstructionFlags.SerializeGameRelease);
		CopyMatchingFields(releaseFields, editorFields);
	}

	public override void ReadEditor(ref EndianSpanReader reader)
	{
		editorFields.Read(ref reader, version, TransferInstructionFlags.NoTransferInstructionFlags);
		CopyMatchingFields(editorFields, releaseFields);
	}

	public override void WriteRelease(AssetWriter writer) => releaseFields.WriteRelease(writer);
	public override void WriteEditor(AssetWriter writer) => editorFields.WriteEditor(writer);
	public override void WalkRelease(AssetWalker walker) => releaseFields.WalkRelease(walker);
	public override void WalkEditor(AssetWalker walker) => editorFields.WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);
	public override IEnumerable<(string, PPtr)> FetchDependencies() => editorFields.FetchDependencies();

	public override void Reset()
	{
		releaseFields.Reset();
		editorFields.Reset();
	}

	public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
		if (source is GUIStyleAsset style)
		{
			releaseFields.CopyValues(style.releaseFields, converter);
			editorFields.CopyValues(style.editorFields, converter);
		}
		else
		{
			Reset();
		}
	}

	private static void CopyMatchingFields(SerializableStructure source, SerializableStructure target)
	{
		for (int i = 0; i < target.Type.Fields.Count; i++)
		{
			if (source.TryGetField(target.Type.Fields[i].Name, out SerializableValue value))
			{
				if (value.CValue is SerializableStructure sourceStructure && target.Fields[i].CValue is SerializableStructure targetStructure)
				{
					CopyMatchingFields(sourceStructure, targetStructure);
				}
				else
				{
					target.Fields[i] = value;
				}
			}
		}
	}

	private static TypeTreeNodeStruct MakeTree(bool editor)
	{
		List<TypeTreeNodeStruct> fields = [String("m_Name")];
		foreach (string name in new[] { "m_Normal", "m_Hover", "m_Active", "m_Focused", "m_OnNormal", "m_OnHover", "m_OnActive", "m_OnFocused" })
		{
			List<TypeTreeNodeStruct> state = [Pointer("Texture2D", "m_Background")];
			if (editor)
			{
				state.Add(Node("vector", "m_ScaledBackgrounds", TransferMetaFlags.AnyChildUsesAlignBytes,
					Node("Array", "Array", TransferMetaFlags.AlignBytes, Scalar("int", "size"), Pointer("Texture2D", "data"))));
			}
			state.Add(Node("ColorRGBA", "m_TextColor", TransferMetaFlags.TransferUsingFlowMappingStyle,
				Scalar("float", "r"), Scalar("float", "g"), Scalar("float", "b"), Scalar("float", "a")));
			fields.Add(Node("GUIStyleState", name, TransferMetaFlags.AnyChildUsesAlignBytes, state.ToArray()));
		}
		foreach (string name in new[] { "m_Border", "m_Margin", "m_Padding", "m_Overflow" })
		{
			fields.Add(Node("RectOffset", name, TransferMetaFlags.NoTransferFlags,
				Scalar("int", "m_Left"), Scalar("int", "m_Right"), Scalar("int", "m_Top"), Scalar("int", "m_Bottom")));
		}
		fields.Add(Pointer("Font", "m_Font"));
		foreach (string name in new[] { "m_FontSize", "m_FontStyle", "m_Alignment" }) fields.Add(Scalar("int", name));
		fields.Add(Scalar("bool", "m_WordWrap"));
		fields.Add(Scalar("bool", "m_RichText", true));
		fields.Add(Scalar("int", "m_TextClipping"));
		fields.Add(Scalar("int", "m_ImagePosition"));
		fields.Add(Node("Vector2f", "m_ContentOffset", TransferMetaFlags.TransferUsingFlowMappingStyle, Scalar("float", "x"), Scalar("float", "y")));
		foreach (string name in new[] { "m_ContentSpacing", "m_FixedWidth", "m_FixedHeight" }) fields.Add(Scalar("float", name));
		foreach (string name in new[] { "m_ImageIsTopAligned", "m_StretchWidth", "m_StretchHeight" }) fields.Add(Scalar("bool", name));
		fields.Add(Scalar("bool", "m_IsSDF", true));
		return Node("GUIStyle", "Base", TransferMetaFlags.AnyChildUsesAlignBytes, fields.ToArray());
	}

	private static TypeTreeNodeStruct Node(string type, string name, TransferMetaFlags flags, params TypeTreeNodeStruct[] children) => new(type, name, 1, flags, children);
	private static TypeTreeNodeStruct Scalar(string type, string name, bool align = false) => Node(type, name, align ? TransferMetaFlags.AlignBytes : TransferMetaFlags.NoTransferFlags);
	private static TypeTreeNodeStruct Pointer(string type, string name) => Node($"PPtr<{type}>", name, TransferMetaFlags.NoTransferFlags, Scalar("int", "m_FileID"), Scalar("SInt64", "m_PathID"));
	private static TypeTreeNodeStruct String(string name) => Node("string", name, TransferMetaFlags.AlignBytes,
		Node("Array", "Array", TransferMetaFlags.AlignBytes, Scalar("int", "size"), Scalar("char", "data")));
}
