using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Structure.Assembly.TypeTrees;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Import.AssetCreation;

/// <summary>
/// Creates a UnityConnectSettings asset for Chinese Unity builds (versions with a "c" type, eg 2022.3.20f1c1).
/// </summary>
/// <remarks>
/// Chinese Unity builds are based on a much later engine than their version number suggests, so their serialized
/// layouts can differ from international builds with the same version number. In particular, the China fork adds
/// two extra URL strings to UnityConnectSettings for its own services. These fields don't exist in any
/// international type tree, so we build the tree manually from the international layout the China fork is based
/// on, inserting the extra fields where the China fork places them.
/// </remarks>
internal static class ChinaUnityConnectSettingsTree
{
	/// <summary>
	/// The international layout on which the China fork's UnityConnectSettings is based.
	/// </summary>
	private static readonly UnityVersion BaseLayoutVersion = new(2022, 2, 0, UnityVersionType.Beta, 8);

	public static TypeTreeObject Create(AssetInfo assetInfo)
	{
		if (TypeTreeNodeStruct.TryMakeFromTpk(ClassIDType.UnityConnectSettings, BaseLayoutVersion, out TypeTreeNodeStruct releaseRoot, out TypeTreeNodeStruct editorRoot))
		{
			return TypeTreeObject.Create(assetInfo, InsertChinaUrlFields(releaseRoot), InsertChinaUrlFields(editorRoot));
		}
		else
		{
			throw new InvalidOperationException($"Could not find the UnityConnectSettings type tree for version {BaseLayoutVersion}.");
		}
	}

	private static TypeTreeNodeStruct InsertChinaUrlFields(TypeTreeNodeStruct root)
	{
		TypeTreeNodeStruct[] subNodes = root.SubNodes.ToArray();
		int insertionPoint = Array.FindIndex(subNodes, static node => node.Name == "m_TestInitMode");
		if (insertionPoint < 0)
		{
			// The base layout doesn't match expectations, so just use it as is.
			return root;
		}

		TypeTreeNodeStruct[] newSubNodes = new TypeTreeNodeStruct[subNodes.Length + 2];
		Array.Copy(subNodes, 0, newSubNodes, 0, insertionPoint);
		newSubNodes[insertionPoint] = ChinaUrlField("m_ChinaEventUrl");
		newSubNodes[insertionPoint + 1] = ChinaUrlField("m_ChinaConfigUrl");
		Array.Copy(subNodes, insertionPoint, newSubNodes, insertionPoint + 2, subNodes.Length - insertionPoint);
		return new TypeTreeNodeStruct(root.TypeName, root.Name, root.Version, root.MetaFlag, newSubNodes);
	}

	private static TypeTreeNodeStruct ChinaUrlField(string name)
	{
		return new TypeTreeNodeStruct(
			"string",
			name,
			1,
			TransferMetaFlags.AnyChildUsesAlignBytes,
			[
				new TypeTreeNodeStruct(
					"Array",
					"Array",
					1,
					TransferMetaFlags.AlignBytes | TransferMetaFlags.HideInEditor,
					[
						new TypeTreeNodeStruct("int", "size", 1, TransferMetaFlags.HideInEditor, []),
						new TypeTreeNodeStruct("char", "data", 1, TransferMetaFlags.HideInEditor, []),
					]),
			]);
	}
}
