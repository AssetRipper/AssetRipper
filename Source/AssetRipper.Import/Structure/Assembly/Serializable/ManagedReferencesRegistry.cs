using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

/// <summary>
/// Runtime representation of Unity's <c>ManagedReferencesRegistry</c> used by <c>[SerializeReference]</c>.
/// </summary>
public sealed class ManagedReferencesRegistry : UnityAssetBase, IDeepCloneable
{
	public const long UnknownRid = -1;
	public const long NullRid = -2;
	public const string TerminusClass = "Terminus";
	public const string TerminusNamespace = "UnityEngine.DMAT";
	public const string TerminusAssembly = "FAKE_ASM";

	public int Version { get; set; } = 2;
	public List<ReferencedObject> RefIds { get; } = [];

	public override void WalkEditor(AssetWalker walker)
	{
		if (!walker.EnterAsset(this))
		{
			return;
		}

		if (walker.EnterField(this, "version"))
		{
			walker.VisitPrimitive(Version);
			walker.ExitField(this, "version");
		}

		walker.DivideAsset(this);

		if (walker.EnterField(this, "RefIds"))
		{
			if (walker.EnterList(RefIds))
			{
				for (int i = 0; i < RefIds.Count; i++)
				{
					if (i > 0)
					{
						walker.DivideList(RefIds);
					}
					RefIds[i].WalkEditor(walker);
				}
				walker.ExitList(RefIds);
			}
			walker.ExitField(this, "RefIds");
		}

		walker.ExitAsset(this);
	}

	public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		foreach (ReferencedObject referencedObject in RefIds)
		{
			foreach ((string, PPtr) pair in referencedObject.FetchDependencies())
			{
				yield return pair;
			}
		}
	}

	public override void Reset()
	{
		Version = 2;
		RefIds.Clear();
	}

	public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
		if (source is not ManagedReferencesRegistry other)
		{
			Reset();
			return;
		}

		Version = other.Version;
		RefIds.Clear();
		foreach (ReferencedObject referencedObject in other.RefIds)
		{
			RefIds.Add(referencedObject.DeepClone(converter));
		}
	}

	public ManagedReferencesRegistry DeepClone(PPtrConverter converter)
	{
		ManagedReferencesRegistry clone = new();
		clone.CopyValues(this, converter);
		return clone;
	}

	IUnityAssetBase IDeepCloneable.DeepClone(PPtrConverter converter) => DeepClone(converter);

	public static ManagedReferencesRegistry Read(
		ref EndianSpanReader reader,
		UnityVersion version,
		TransferInstructionFlags flags,
		int depth,
		IReadOnlyList<SerializedTypeReference> refTypes,
		IAssemblyManager? assemblyManager)
	{
		ManagedReferencesRegistry registry = new()
		{
			Version = reader.ReadInt32(),
		};

		if (registry.Version == 1)
		{
			int index = 0;
			while (true)
			{
				ReferencedObject? referencedObject = ReferencedObject.Read(
					ref reader,
					version,
					flags,
					depth,
					refTypes,
					assemblyManager,
					explicitRid: null,
					fallbackRid: index++);
				if (referencedObject is null)
				{
					break;
				}
				registry.RefIds.Add(referencedObject);
			}
		}
		else if (registry.Version == 2)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				long rid = reader.ReadInt64();
				ReferencedObject? referencedObject = ReferencedObject.Read(
					ref reader,
					version,
					flags,
					depth,
					refTypes,
					assemblyManager,
					explicitRid: rid,
					fallbackRid: rid);
				if (referencedObject is not null)
				{
					registry.RefIds.Add(referencedObject);
				}
			}
		}
		else
		{
			throw new NotSupportedException($"Unsupported ManagedReferencesRegistry version {registry.Version}.");
		}

		return registry;
	}
}

public sealed class ReferencedObject : UnityAssetBase, IDeepCloneable
{
	public long Rid { get; set; }
	public ReferencedManagedType Type { get; set; } = new();
	public IUnityAssetBase? Data { get; set; }

	public override void WalkEditor(AssetWalker walker)
	{
		if (!walker.EnterAsset(this))
		{
			return;
		}

		if (walker.EnterField(this, "rid"))
		{
			walker.VisitPrimitive(Rid);
			walker.ExitField(this, "rid");
		}

		walker.DivideAsset(this);

		if (walker.EnterField(this, "type"))
		{
			Type.WalkEditor(walker);
			walker.ExitField(this, "type");
		}

		walker.DivideAsset(this);

		if (walker.EnterField(this, "data"))
		{
			if (Data is not null)
			{
				Data.WalkEditor(walker);
			}
			else
			{
				// Emit an empty mapping so Unity accepts empty [Serializable] payloads.
				EmptyReferencedObjectData.Instance.WalkEditor(walker);
			}
			walker.ExitField(this, "data");
		}

		walker.ExitAsset(this);
	}

	public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		if (Data is null)
		{
			yield break;
		}

		foreach ((string, PPtr) pair in Data.FetchDependencies())
		{
			yield return pair;
		}
	}

	public override void Reset()
	{
		Rid = 0;
		Type.Reset();
		Data = null;
	}

	public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
		if (source is not ReferencedObject other)
		{
			Reset();
			return;
		}

		Rid = other.Rid;
		Type.CopyValues(other.Type, converter);
		if (other.Data is IDeepCloneable cloneable)
		{
			Data = cloneable.DeepClone(converter);
		}
		else if (other.Data is SerializableStructure structure)
		{
			Data = structure.DeepClone(converter);
		}
		else
		{
			Data = null;
		}
	}

	public ReferencedObject DeepClone(PPtrConverter converter)
	{
		ReferencedObject clone = new();
		clone.CopyValues(this, converter);
		return clone;
	}

	IUnityAssetBase IDeepCloneable.DeepClone(PPtrConverter converter) => DeepClone(converter);

	/// <summary>
	/// Reads one referenced object. Returns <see langword="null"/> when the v1 terminus sentinel is reached.
	/// </summary>
	public static ReferencedObject? Read(
		ref EndianSpanReader reader,
		UnityVersion version,
		TransferInstructionFlags flags,
		int depth,
		IReadOnlyList<SerializedTypeReference> refTypes,
		IAssemblyManager? assemblyManager,
		long? explicitRid,
		long fallbackRid)
	{
		ReferencedManagedType type = ReferencedManagedType.Read(ref reader);
		if (type.IsTerminus)
		{
			return null;
		}

		ReferencedObject referencedObject = new()
		{
			Rid = explicitRid ?? fallbackRid,
			Type = type,
		};

		if (referencedObject.Rid is ManagedReferencesRegistry.UnknownRid or ManagedReferencesRegistry.NullRid
			|| type.IsEmpty)
		{
			referencedObject.Data = null;
			return referencedObject;
		}

		referencedObject.Data = ReadReferencedObjectData(
			ref reader,
			version,
			flags,
			depth,
			type,
			refTypes,
			assemblyManager);
		return referencedObject;
	}

	private static IUnityAssetBase? ReadReferencedObjectData(
		ref EndianSpanReader reader,
		UnityVersion version,
		TransferInstructionFlags flags,
		int depth,
		ReferencedManagedType type,
		IReadOnlyList<SerializedTypeReference> refTypes,
		IAssemblyManager? assemblyManager)
	{
		SerializedTypeReference? refType = FindRefType(refTypes, type);
		if (refType is not null && TypeTreeNodeStruct.TryMakeFromTypeTree(refType.OldType, out TypeTreeNodeStruct rootNode))
		{
			SerializableStructure structure = SerializableTreeType.FromRootNode(rootNode).CreateSerializableStructure();
			structure.ReadReferencedObject(ref reader, version, flags);
			return structure;
		}

		if (assemblyManager is not null && !type.IsEmpty)
		{
			string assemblyName = SpecialFileNames.FixAssemblyName(type.AsmName);
			ScriptIdentifier scriptID = assemblyManager.GetScriptID(assemblyName, type.Namespace, type.ClassName);
			if (assemblyManager.TryGetSerializableType(scriptID, version, out SerializableType? serializableType, out _))
			{
				SerializableStructure structure = serializableType.CreateSerializableStructure();
				structure.ReadReferencedObject(ref reader, version, flags);
				return structure;
			}
		}

		// Unknown type with no layout information: leave unread bytes will fail the parent length check.
		throw new InvalidDataException(
			$"Unable to resolve SerializeReference type [{type.AsmName}]{type.Namespace}.{type.ClassName}.");
	}

	private static SerializedTypeReference? FindRefType(IReadOnlyList<SerializedTypeReference> refTypes, ReferencedManagedType type)
	{
		foreach (SerializedTypeReference refType in refTypes)
		{
			if (refType.ClassName == type.ClassName
				&& refType.Namespace == type.Namespace
				&& refType.AsmName == type.AsmName)
			{
				return refType;
			}
		}
		return null;
	}
}

public sealed class ReferencedManagedType : UnityAssetBase, IDeepCloneable
{
	public string ClassName { get; set; } = "";
	public string Namespace { get; set; } = "";
	public string AsmName { get; set; } = "";

	public bool IsEmpty => string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(AsmName);

	public bool IsTerminus =>
		ClassName == ManagedReferencesRegistry.TerminusClass
		&& Namespace == ManagedReferencesRegistry.TerminusNamespace
		&& AsmName == ManagedReferencesRegistry.TerminusAssembly;

	public override bool FlowMappedInYaml => true;

	public override void WalkEditor(AssetWalker walker)
	{
		if (!walker.EnterAsset(this))
		{
			return;
		}

		if (walker.EnterField(this, "class"))
		{
			walker.VisitPrimitive(ClassName);
			walker.ExitField(this, "class");
		}

		walker.DivideAsset(this);

		if (walker.EnterField(this, "ns"))
		{
			walker.VisitPrimitive(Namespace);
			walker.ExitField(this, "ns");
		}

		walker.DivideAsset(this);

		if (walker.EnterField(this, "asm"))
		{
			walker.VisitPrimitive(AsmName);
			walker.ExitField(this, "asm");
		}

		walker.ExitAsset(this);
	}

	public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

	public override void Reset()
	{
		ClassName = "";
		Namespace = "";
		AsmName = "";
	}

	public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
		if (source is not ReferencedManagedType other)
		{
			Reset();
			return;
		}

		ClassName = other.ClassName;
		Namespace = other.Namespace;
		AsmName = other.AsmName;
	}

	public ReferencedManagedType DeepClone(PPtrConverter converter)
	{
		ReferencedManagedType clone = new();
		clone.CopyValues(this, converter);
		return clone;
	}

	IUnityAssetBase IDeepCloneable.DeepClone(PPtrConverter converter) => DeepClone(converter);

	public static ReferencedManagedType Read(ref EndianSpanReader reader)
	{
		return new ReferencedManagedType
		{
			ClassName = reader.ReadUtf8StringAligned().String,
			Namespace = reader.ReadUtf8StringAligned().String,
			AsmName = reader.ReadUtf8StringAligned().String,
		};
	}
}

file sealed class EmptyReferencedObjectData : UnityAssetBase
{
	public static EmptyReferencedObjectData Instance { get; } = new();

	public override void WalkEditor(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			walker.ExitAsset(this);
		}
	}

	public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

	public override void Reset()
	{
	}
}
