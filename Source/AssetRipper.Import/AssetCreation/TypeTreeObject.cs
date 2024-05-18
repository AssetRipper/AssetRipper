using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;

namespace AssetRipper.Import.AssetCreation;

public sealed class TypeTreeObject : NullObject
{
	public bool IsPlayerSettings => ClassID == 129;
	public SerializableStructure ReleaseFields { get; }
	public SerializableStructure EditorFields { get; }
	public TypeTreeObject(TypeTreeNodeStruct releaseRoot, TypeTreeNodeStruct editorRoot, AssetInfo assetInfo) : base(assetInfo)
	{
		ReleaseFields = SerializableTreeType.FromRootNode(releaseRoot).CreateSerializableStructure();
		EditorFields = SerializableTreeType.FromRootNode(editorRoot).CreateSerializableStructure();
	}

	public override void ReadRelease(ref EndianSpanReader reader)
	{
		ReleaseFields.Read(ref reader, Collection.Version, Collection.Flags);
		ConvertFields(ReleaseFields, EditorFields);
	}

	public override void ReadEditor(ref EndianSpanReader reader)
	{
		EditorFields.Read(ref reader, Collection.Version, Collection.Flags);
		ConvertFields(EditorFields, ReleaseFields);
	}

	public override void WriteRelease(AssetWriter writer) => ReleaseFields.WriteRelease(writer);

	public override void WriteEditor(AssetWriter writer) => EditorFields.WriteEditor(writer);

	public override void WalkRelease(AssetWalker walker) => ReleaseFields.WalkRelease(walker);

	public override void WalkEditor(AssetWalker walker) => EditorFields.WalkEditor(walker);

	public override bool FlowMappedInYaml => EditorFields.FlowMappedInYaml;

	public override int SerializedVersion => EditorFields.SerializedVersion;

	public override void Reset()
	{
		ReleaseFields.Reset();
		EditorFields.Reset();
	}

	public override string ClassName => EditorFields.Type.Name;

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		return ReleaseFields.FetchDependencies().Union(EditorFields.FetchDependencies());
	}

	private void ConvertFields(SerializableStructure source,  SerializableStructure target)
	{
		target.CopyValues(source, new PPtrConverter(Collection, Collection));
	}
}
