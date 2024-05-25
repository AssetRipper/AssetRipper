using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;

namespace AssetRipper.Import.AssetCreation;

public abstract class TypeTreeObject : NullObject
{
	public bool IsPlayerSettings => ClassID == 129;
	public abstract SerializableStructure ReleaseFields { get; }
	public abstract SerializableStructure EditorFields { get; }
	public sealed override bool FlowMappedInYaml => EditorFields.FlowMappedInYaml;
	public sealed override int SerializedVersion => EditorFields.SerializedVersion;
	public sealed override string ClassName => EditorFields.Type.Name;

	private TypeTreeObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public sealed override void WriteRelease(AssetWriter writer) => ReleaseFields.WriteRelease(writer);

	public sealed override void WriteEditor(AssetWriter writer) => EditorFields.WriteEditor(writer);

	public sealed override void WalkRelease(AssetWalker walker) => ReleaseFields.WalkRelease(walker);

	public sealed override void WalkEditor(AssetWalker walker) => EditorFields.WalkEditor(walker);

	public static TypeTreeObject Create(TypeTreeNodeStruct root, AssetInfo assetInfo) => new SingleTypeTreeObject(root, assetInfo);

	public static TypeTreeObject Create(TypeTreeNodeStruct releaseRoot, TypeTreeNodeStruct editorRoot, AssetInfo assetInfo) => new DoubleTypeTreeObject(releaseRoot, editorRoot, assetInfo);

	private sealed class SingleTypeTreeObject : TypeTreeObject
	{
		public SerializableStructure Fields { get; }
		public override SerializableStructure ReleaseFields => Fields;
		public override SerializableStructure EditorFields => Fields;

		public SingleTypeTreeObject(TypeTreeNodeStruct root, AssetInfo assetInfo) : base(assetInfo)
		{
			Fields = SerializableTreeType.FromRootNode(root).CreateSerializableStructure();
		}

		public override void ReadRelease(ref EndianSpanReader reader)
		{
			Fields.Read(ref reader, Collection.Version, Collection.Flags);
		}

		public override void ReadEditor(ref EndianSpanReader reader)
		{
			Fields.Read(ref reader, Collection.Version, Collection.Flags);
		}

		public override void WalkStandard(AssetWalker walker)
		{
			Fields.WalkStandard(walker);
		}

		public override void Reset()
		{
			Fields.Reset();
		}

		public override IEnumerable<(string, PPtr)> FetchDependencies()
		{
			return Fields.FetchDependencies();
		}
	}

	private sealed class DoubleTypeTreeObject : TypeTreeObject
	{
		public override SerializableStructure ReleaseFields { get; }
		public override SerializableStructure EditorFields { get; }

		public DoubleTypeTreeObject(TypeTreeNodeStruct releaseRoot, TypeTreeNodeStruct editorRoot, AssetInfo assetInfo) : base(assetInfo)
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

		public override void Reset()
		{
			ReleaseFields.Reset();
			EditorFields.Reset();
		}

		public override IEnumerable<(string, PPtr)> FetchDependencies()
		{
			return ReleaseFields.FetchDependencies().Union(EditorFields.FetchDependencies());
		}

		private void ConvertFields(SerializableStructure source, SerializableStructure target)
		{
			target.CopyValues(source, new PPtrConverter(Collection, Collection));
		}
	}
}
