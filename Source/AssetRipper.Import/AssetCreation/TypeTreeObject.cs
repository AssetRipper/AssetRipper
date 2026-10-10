using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;
using System.Diagnostics;

namespace AssetRipper.Import.AssetCreation;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public abstract class TypeTreeObject : NullObject
{
	private uint? playerSettingsRuntimeWord;
	public bool IsPlayerSettings => ClassID == 129;
	public bool IsGlobalGameManager => IsPlayerSettings || ClassID == 55;
	public abstract SerializableStructure ReleaseFields { get; }
	public abstract SerializableStructure EditorFields { get; }
	public sealed override bool FlowMappedInYaml => EditorFields.FlowMappedInYaml;
	public sealed override int SerializedVersion => EditorFields.SerializedVersion;
	public sealed override string ClassName => EditorFields.Type.Name;

	private TypeTreeObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public sealed override void WriteRelease(AssetWriter writer)
	{
		if (playerSettingsRuntimeWord is not uint runtimeWord)
		{
			ReleaseFields.WriteRelease(writer);
			return;
		}
		for (int i = 0; i < ReleaseFields.Type.Fields.Count; i++)
		{
			var field = ReleaseFields.Type.Fields[i];
			if (field.Name == "allowedHttpConnections") writer.Write(runtimeWord);
			ReleaseFields.Fields[i].Write(writer, field);
		}
	}

	private void CompletePlayerSettingsRead(ref EndianSpanReader reader)
	{
		// The 6000.5.10f1 player stream contains an additional word immediately
		// before allowedHttpConnections that is absent from its generated type tree.
		// Keep it independently of the editor fields so binary writing is lossless.
		if (IsPlayerSettings && Collection.Version == UnityVersion.Parse("6000.5.10f1")
			&& reader.Length - reader.Position == sizeof(int)
			&& ReleaseFields.Type.Fields[^1].Name == "allowedHttpConnections")
		{
			ref SerializableValue value = ref ReleaseFields["allowedHttpConnections"];
			int connections = reader.ReadInt32();
			if (value.AsInt32 != 0 || connections is < 0 or > 3)
			{
				throw new InvalidDataException("Unexpected Unity 6000.5.10f1 PlayerSettings runtime extension.");
			}
			playerSettingsRuntimeWord = value.AsUInt32;
			value.AsInt32 = connections;
		}
	}

	public sealed override void WriteEditor(AssetWriter writer) => EditorFields.WriteEditor(writer);

	public sealed override void WalkRelease(AssetWalker walker) => ReleaseFields.WalkRelease(walker);

	public sealed override void WalkEditor(AssetWalker walker) => EditorFields.WalkEditor(walker);

	public static TypeTreeObject Create(AssetInfo assetInfo, TypeTreeNodeStruct root, ITypeResolver resolver) => new SingleTypeTreeObject(assetInfo, root, resolver);

	public static TypeTreeObject Create(AssetInfo assetInfo, TypeTreeNodeStruct releaseRoot, TypeTreeNodeStruct editorRoot, ITypeResolver resolver) => new DoubleTypeTreeObject(assetInfo, releaseRoot, editorRoot, resolver);

	private string GetDebuggerDisplay() => ClassName;

	private sealed class SingleTypeTreeObject : TypeTreeObject
	{
		private readonly ITypeResolver resolver;
		public SerializableStructure Fields { get; }
		public override SerializableStructure ReleaseFields => Fields;
		public override SerializableStructure EditorFields => Fields;

		public SingleTypeTreeObject(AssetInfo assetInfo, TypeTreeNodeStruct root, ITypeResolver resolver) : base(assetInfo)
		{
			this.resolver = resolver;
			Fields = SerializableTreeType.FromRootNode(root).CreateSerializableStructure();
		}

		public override void ReadRelease(ref EndianSpanReader reader)
		{
			Fields.Read(ref reader, Collection.Version, Collection.Flags, resolver);
		}

		public override void ReadEditor(ref EndianSpanReader reader)
		{
			Fields.Read(ref reader, Collection.Version, Collection.Flags, resolver);
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
		private readonly ITypeResolver resolver;
		public override SerializableStructure ReleaseFields { get; }
		public override SerializableStructure EditorFields { get; }

		public DoubleTypeTreeObject(AssetInfo assetInfo, TypeTreeNodeStruct releaseRoot, TypeTreeNodeStruct editorRoot, ITypeResolver resolver) : base(assetInfo)
		{
			this.resolver = resolver;
			ReleaseFields = SerializableTreeType.FromRootNode(releaseRoot).CreateSerializableStructure();
			EditorFields = SerializableTreeType.FromRootNode(editorRoot).CreateSerializableStructure();
		}

		public override void ReadRelease(ref EndianSpanReader reader)
		{
			playerSettingsRuntimeWord = null;
			ReleaseFields.Read(ref reader, Collection.Version, Collection.Flags, resolver);
			CompletePlayerSettingsRead(ref reader);
			ConvertFields(ReleaseFields, EditorFields);
		}

		public override void ReadEditor(ref EndianSpanReader reader)
		{
			playerSettingsRuntimeWord = null;
			EditorFields.Read(ref reader, Collection.Version, Collection.Flags, resolver);
			ConvertFields(EditorFields, ReleaseFields);
		}

		public override void WalkStandard(AssetWalker walker)
		{
			if (walker.EnterAsset(this))
			{
				if (walker.EnterField(this, "Release"))
				{
					ReleaseFields.WalkStandard(walker);
					walker.ExitField(this, "Release");
				}
				walker.DivideAsset(this);
				if (walker.EnterField(this, "Editor"))
				{
					EditorFields.WalkStandard(walker);
					walker.ExitField(this, "Editor");
				}
				walker.ExitAsset(this);
			}
		}

		public override void Reset()
		{
			playerSettingsRuntimeWord = null;
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
