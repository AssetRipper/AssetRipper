using AssetRipper.Yaml;

namespace AssetRipper.Export.UnityProjects;

public readonly record struct MetaPtr(long FileID, UnityGuid GUID, AssetType AssetType)
{
	public MetaPtr(long fileID) : this(fileID, UnityGuid.Zero, AssetType.Serialized)
	{
	}

	public YamlNode ExportYaml(UnityVersion exportVersion)
	{
		YamlMappingNode node = new();
		node.Style = MappingStyle.Flow;
		node.Add(FileIDName, FileID);
		if (!GUID.IsZero)
		{
			node.Add(GuidName, GUID.ToString());
			if (exportVersion.GreaterThanOrEquals(4) || AssetType is not AssetType.Meta)
			{
				node.Add(TypeName, (int)AssetType);
			}
			else
			{
				// For Unity 3, type 3 (Meta) is only used for 3d models.
				// All other imported assets (eg images and audio) use type 1 (Cached).
				// Since we only export yaml meshes during Unity project export and have no plans to change that,
				// we can safely redirect all type 3 references to type 1.
				// https://github.com/AssetRipper/AssetRipper/issues/1827
				// https://github.com/AssetRipper/AssetRipper/issues/1329
				node.Add(TypeName, (int)AssetType.Cached);
			}
		}
		return node;
	}

	/// <summary>
	/// Write this pointer to a string.
	/// </summary>
	/// <remarks>
	/// This uses the same format as Yaml.
	/// </remarks>
	/// <returns>This pointer expressed as a string.</returns>
	public override string ToString()
	{
		if (GUID.IsZero)
		{
			return $"{{{FileIDName}: {FileID}}}";
		}
		else
		{
			return $"{{{FileIDName}: {FileID}, {GuidName}: {GUID}, {TypeName}: {(int)AssetType}}}";
		}
	}

	public static MetaPtr NullPtr { get; } = new MetaPtr(0);

	public static MetaPtr CreateMissingReference(int classID, AssetType assetType)
	{
		return new MetaPtr(ExportIdHandler.GetMainExportID(classID), UnityGuid.MissingReference, assetType);
	}

	private const string FileIDName = "fileID";
	private const string GuidName = "guid";
	private const string TypeName = "type";
}
