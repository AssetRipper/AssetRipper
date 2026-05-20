using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SerializationLogic;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

internal sealed class TypeTreeResolver : ITypeResolver
{
	private readonly IReadOnlyList<SerializedTypeReference> m_refTypes;

	public TypeTreeResolver(IReadOnlyList<SerializedTypeReference> refTypes)
	{
		m_refTypes = refTypes;
	}

	public bool TryGetSerializableType(
		ScriptIdentifier scriptID,
		UnityVersion version,
		[NotNullWhen(true)] out SerializableType? scriptType,
		[NotNullWhen(false)] out string? failureReason)
	{
		for (int i = 0; i < m_refTypes.Count; i++)
		{
			SerializedTypeReference refType = m_refTypes[i];
			if (refType.ClassName.String == scriptID.Name &&
				refType.Namespace.String == scriptID.Namespace &&
				refType.AsmName.String == scriptID.Assembly)
			{
				if (TypeTreeNodeStruct.TryMakeFromTypeTree(refType.OldType, out TypeTreeNodeStruct rootNode))
				{
					scriptType = SerializableTreeType.FromRootNode(rootNode, true);
					failureReason = null;
					return true;
				}
				else
				{
					scriptType = null;
					failureReason = "Failed to create nodes from the type tree";
					return false;
				}
			}
		}

		scriptType = null;
		failureReason = "Serialized type reference was not found in the set of available type trees.";
		return false;
	}
}
