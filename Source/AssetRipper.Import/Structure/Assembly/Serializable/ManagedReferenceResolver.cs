using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

internal sealed class ManagedReferenceResolver
{
	public static ManagedReferenceTypeKey TerminusKey { get; } = new("FAKE_ASM", "UnityEngine.DMAT", "Terminus");

	private readonly Dictionary<ManagedReferenceTypeKey, SerializableType?> cache = [];
	private readonly SerializedTypeReference[] refTypes;
	private readonly IAssemblyManager assemblyManager;
	private readonly UnityVersion version;
	private readonly Dictionary<ITypeDefOrRef, SerializableType> assemblyTypeCache = new(SignatureComparer.Default);

	public ManagedReferenceResolver(SerializedAssetCollection? collection, IAssemblyManager assemblyManager, UnityVersion version)
	{
		refTypes = collection?.RefTypes ?? [];
		this.assemblyManager = assemblyManager;
		this.version = version;
	}

	public SerializableType? Resolve(ManagedReferenceTypeDescriptor descriptor)
	{
		ManagedReferenceTypeKey key = descriptor.Key.Normalize();
		if (key.IsEmpty || descriptor.IsTerminus)
		{
			return null;
		}
		if (cache.TryGetValue(key, out SerializableType? cached))
		{
			return cached;
		}

		SerializableType? resolved = ResolveFromRefTypes(key) ?? ResolveFromAssemblies(key);
		cache[key] = resolved;
		return resolved;
	}

	private SerializableType? ResolveFromRefTypes(ManagedReferenceTypeKey key)
	{
		foreach (SerializedTypeReference refType in refTypes)
		{
			if (refType.ClassName.String == key.ClassName
				&& refType.Namespace.String == key.Namespace
				&& ManagedReferenceTypeKey.NormalizeAssemblyName(refType.AsmName.String) == key.AssemblyName
				&& TypeTreeNodeStruct.TryMakeFromTypeTree(refType.OldType, out TypeTreeNodeStruct rootNode))
			{
				rootNode = RemoveRegistry(rootNode);
				return SerializableTreeType.FromRootNode(rootNode).CreateSerializableStructure().Type;
			}
		}
		return null;
	}

	private SerializableType? ResolveFromAssemblies(ManagedReferenceTypeKey key)
	{
		if (!assemblyManager.IsSet)
		{
			return null;
		}

		ScriptIdentifier scriptId = assemblyManager.GetScriptID(key.AssemblyName, key.Namespace, key.ClassName);
		if (!assemblyManager.IsValid(scriptId))
		{
			return null;
		}

		TypeDefinition type = assemblyManager.GetTypeDefinition(scriptId);
		FieldSerializer serializer = new(version);
		return serializer.TryCreateSerializableType(type, assemblyTypeCache, out SerializableType? result, out _)
			? result
			: null;
	}

	private static TypeTreeNodeStruct RemoveRegistry(TypeTreeNodeStruct rootNode)
	{
		if (rootNode.SubNodes.Count == 0 || !rootNode.SubNodes[^1].IsManagedReferencesRegistry)
		{
			return rootNode;
		}
		TypeTreeNodeStruct[] filtered = rootNode.SubNodes.Take(rootNode.SubNodes.Count - 1).ToArray();
		return new TypeTreeNodeStruct(rootNode.TypeName, rootNode.Name, rootNode.Version, rootNode.MetaFlag, filtered);
	}
}
