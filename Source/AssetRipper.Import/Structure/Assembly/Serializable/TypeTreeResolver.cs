using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

/// <summary>
/// An <see cref="ITypeResolver"/> backed by the reference type trees of a <see cref="IO.Files.SerializedFiles.SerializedFile"/>.
/// </summary>
internal sealed class TypeTreeResolver : ITypeResolver
{
	private static readonly TypeTreeResolver empty = new([]);
	private readonly IReadOnlyList<SerializedTypeReference> referenceTypes;

	private TypeTreeResolver(IReadOnlyList<SerializedTypeReference> referenceTypes)
	{
		this.referenceTypes = referenceTypes;
	}

	public static TypeTreeResolver Create(IReadOnlyList<SerializedTypeReference> referenceTypes)
	{
		return referenceTypes.Count == 0 ? empty : new TypeTreeResolver(referenceTypes);
	}

	public bool TryGetSerializableType(
		ScriptIdentifier scriptID,
		UnityVersion version,
		[NotNullWhen(true)] out SerializableType? scriptType,
		[NotNullWhen(false)] out string? failureReason)
	{
		for (int i = 0; i < referenceTypes.Count; i++)
		{
			SerializedTypeReference referenceType = referenceTypes[i];
			if (referenceType.ClassName.String != scriptID.Name
				|| referenceType.Namespace.String != scriptID.Namespace
				|| SpecialFileNames.RemoveAssemblyFileExtension(referenceType.AsmName.String) != scriptID.Assembly)
			{
				continue;
			}

			if (TypeTreeNodeStruct.TryMakeFromTypeTree(referenceType.OldType, out TypeTreeNodeStruct rootNode))
			{
				scriptType = SerializableTreeType.FromReferencedObjectNode(rootNode);
				failureReason = null;
				return true;
			}
			else
			{
				scriptType = null;
				failureReason = "Failed to create nodes from the type tree.";
				return false;
			}
		}

		scriptType = null;
		failureReason = "The type reference was not found in the set of available type trees.";
		return false;
	}
}
