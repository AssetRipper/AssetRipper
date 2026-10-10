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
		releaseFields = SerializableTreeType.FromRootNode(GUIStyleTypeTree.Create(editor: false)).CreateSerializableStructure();
		editorFields = SerializableTreeType.FromRootNode(GUIStyleTypeTree.Create(editor: true)).CreateSerializableStructure();
		releaseFields.InitializeFields(version);
		editorFields.InitializeFields(version);
	}

	public override void ReadRelease(ref EndianSpanReader reader)
	{
		releaseFields.Read(ref reader, version, TransferInstructionFlags.SerializeGameRelease, ITypeResolver.Null);
		CopyMatchingFields(releaseFields, editorFields);
	}

	public override void ReadEditor(ref EndianSpanReader reader)
	{
		editorFields.Read(ref reader, version, TransferInstructionFlags.NoTransferInstructionFlags, ITypeResolver.Null);
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
}
