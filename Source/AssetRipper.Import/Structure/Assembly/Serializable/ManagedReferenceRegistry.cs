using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;
using AssetRipper.SerializationLogic;
using AsmResolver.DotNet.Signatures.Parsing;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

/// <summary>The version 2 managed reference registry used by modern Unity players.</summary>
public sealed class ManagedReferenceRegistry : UnityAssetBase, IDeepCloneable
{
	public sealed record Entry(long Rid, string Class, string Namespace, string Assembly, SerializableStructure? Data);
	public IReadOnlyList<Entry> Entries { get; private set; }

	private ManagedReferenceRegistry(IReadOnlyList<Entry> entries) => Entries = entries;

	public static ManagedReferenceRegistry Read(ref EndianSpanReader reader, AssetCollection collection, IAssemblyManager? assemblyManager)
	{
		int version = reader.ReadInt32();
		if (version != 2)
		{
			throw new NotSupportedException($"Managed reference registry version {version} is not supported.");
		}
		int count = reader.ReadInt32();
		if (count < 0 || count > (reader.Length - reader.Position) / 20)
		{
			throw new InvalidDataException($"Invalid managed reference count: {count}.");
		}
		Entry[] entries = new Entry[count];
		for (int i = 0; i < count; i++)
		{
			long rid = reader.ReadInt64();
			string className = reader.ReadUtf8StringAligned().String;
			string @namespace = reader.ReadUtf8StringAligned().String;
			string assembly = reader.ReadUtf8StringAligned().String;
			SerializableStructure? data = null;
			if (!string.IsNullOrEmpty(className))
			{
				SerializableType? type = null;
				if (collection is SerializedAssetCollection serialized)
				{
					var reference = serialized.ReferenceTypes.FirstOrDefault(t => t.ClassName == className && t.Namespace == @namespace && t.AsmName == assembly);
					if (reference is not null && TypeTreeNodeStruct.TryMakeFromTypeTree(reference.OldType, out var root))
					{
						type = SerializableTreeType.FromRootNode(root);
					}
				}
				if (type is null && assemblyManager is not null)
				{
					if (className.Contains('['))
					{
						var module = assemblyManager.GetAssemblies().FirstOrDefault(a => a.Name == assembly)?.ManifestModule;
						if (module is not null)
						{
							string fullName = string.IsNullOrEmpty(@namespace) ? className : $"{@namespace}.{className}";
							var signature = TypeNameParser.Parse(module, $"{fullName.Replace('/', '+')}, {assembly}");
							new FieldSerializer(collection.Version, assemblyManager.RuntimeContext).TryCreateSerializableType(signature, out type, out _);
						}
					}
					else
					{
						var scriptID = assemblyManager.GetScriptID(assembly, @namespace, className);
						if (!scriptID.IsDefault)
						{
							assemblyManager.TryGetSerializableType(scriptID, collection.Version, out type, out _);
						}
					}
				}
				if (type is null)
				{
					throw new InvalidDataException($"Cannot resolve managed reference type [{assembly}]{@namespace}.{className} (rid {rid}).");
				}
				data = type.CreateSerializableStructure();
				data.Read(ref reader, collection.Version, collection.Flags);
			}
			entries[i] = new(rid, className, @namespace, assembly, data);
		}
		reader.Align();
		return new(entries);
	}

	public override void WriteRelease(AssetWriter writer)
	{
		writer.Write(2);
		writer.Write(Entries.Count);
		foreach (Entry entry in Entries)
		{
			writer.Write(entry.Rid);
			WriteString(writer, entry.Class);
			WriteString(writer, entry.Namespace);
			WriteString(writer, entry.Assembly);
			entry.Data?.WriteRelease(writer);
		}
		writer.AlignStream();
	}
	public override void WriteEditor(AssetWriter writer) => WriteRelease(writer);

	private static void WriteString(AssetWriter writer, string value)
	{
		writer.Write((AssetRipper.Primitives.Utf8String)value);
		writer.AlignStream();
	}

	public override void WalkEditor(AssetWalker walker) => CreateFields().WalkEditor(walker);
	public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

	private SerializableStructure CreateFields()
	{
		RecordType empty = new("ReferencedObjectData", []);
		RecordType identityType = new("ReferencedManagedType", [StringField("class"), StringField("ns"), StringField("asm")]);
		RecordType entryType = new("ReferencedObject", [new(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Long), 0, "rid", false), new(identityType, 0, "type", false), new(empty, 0, "data", false)]);
		RecordType registryType = new("ManagedReferencesRegistry", [new(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Int), 0, "version", false), new(entryType, 1, "RefIds", false)]);
		var fields = registryType.CreateSerializableStructure();
		fields["version"].AsInt32 = 2;
		fields["RefIds"].AsAssetArray = Entries.Select(entry =>
		{
			var identity = identityType.CreateSerializableStructure();
			identity["class"].AsString = entry.Class;
			identity["ns"].AsString = entry.Namespace;
			identity["asm"].AsString = entry.Assembly;
			var record = entryType.CreateSerializableStructure();
			record["rid"].AsInt64 = entry.Rid;
			record["type"].AsAsset = identity;
			record["data"].AsAsset = entry.Data ?? empty.CreateSerializableStructure();
			return (IUnityAssetBase)record;
		}).ToArray();
		return fields;
	}

	private static SerializableType.Field StringField(string name) => new(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 0, name, true);

	private sealed class RecordType : SerializableType
	{
		public RecordType(string name, IReadOnlyList<Field> fields) : base(null, PrimitiveType.Complex, name)
		{
			Fields = fields;
			MaxDepth = fields.Count == 0 ? 0 : fields.Max(f => f.Type.MaxDepth) + 1;
		}
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies() => Entries.SelectMany(entry => entry.Data?.FetchDependencies() ?? []);
	public ManagedReferenceRegistry DeepClone(PPtrConverter converter) => new(Entries.Select(entry => entry with { Data = entry.Data?.DeepClone(converter) }).ToArray());
	IUnityAssetBase IDeepCloneable.DeepClone(PPtrConverter converter) => DeepClone(converter);
	public override void Reset()
	{
		Entries = [];
	}
}
