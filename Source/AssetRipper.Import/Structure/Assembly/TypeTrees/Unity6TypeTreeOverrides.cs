using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Import.Structure.Assembly.TypeTrees;

/// <summary>
/// Layout changes newer than the bundled generated type trees.
/// </summary>
internal static class Unity6TypeTreeOverrides
{
	public static bool NeedsPhysicsOverride(UnityVersion version) => version.GreaterThanOrEquals(6000, 5);

	public static TypeTreeNodeStruct Apply(ClassIDType classID, UnityVersion version, TypeTreeNodeStruct root)
	{
		if (classID is ClassIDType.PhysicsManager && NeedsPhysicsOverride(version)
			&& !root.SubNodes.Any(node => node.Name == "m_ThreadingMode"))
		{
			List<TypeTreeNodeStruct> fields = new(root.Count + 1);
			foreach (TypeTreeNodeStruct node in root)
			{
				if (node.Name == "m_SimulationMode")
				{
					fields.Add(new(node.TypeName, node.Name, node.Version, node.MetaFlag & ~TransferMetaFlags.AlignBytes, node.SubNodes.ToArray()));
					fields.Add(new("int", "m_ThreadingMode", 1, TransferMetaFlags.AlignBytes, []));
				}
				else
				{
					fields.Add(node);
				}
			}
			return new(root.TypeName, root.Name, 24, root.MetaFlag, fields.ToArray());
		}
		return root;
	}
}
