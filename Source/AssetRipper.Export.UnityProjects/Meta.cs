using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Yaml;


namespace AssetRipper.Export.UnityProjects;

public readonly struct Meta
{
	public Meta(UnityGuid guid, IUnityObjectBase importer) : this(guid, importer, true) { }

	public Meta(UnityGuid guid, IUnityObjectBase importer, bool hasLicense) : this(guid, importer, hasLicense, false) { }

	public Meta(UnityGuid guid, IUnityObjectBase importer, bool hasLicense, bool isFolder)
	{
		if (guid.IsZero)
		{
			throw new ArgumentNullException(nameof(guid));
		}

		GUID = guid;
		IsFolderAsset = isFolder;
		HasLicenseData = hasLicense;
		Importer = importer ?? throw new ArgumentNullException(nameof(importer));
	}

	private static int ToFileFormatVersion()
	{
		//This has been 2 for a long time, but probably not forever.
		//If Unity 3 usesd version 1, we need to find out when 2 started.
		return 2;
	}

	public YamlDocument ExportYamlDocument(IExportContainer container)
	{
		YamlDocument document = new();
		YamlMappingNode root = document.CreateMappingRoot();
		root.Add(FileFormatVersionName, ToFileFormatVersion());
		root.Add(GuidName, GUID.ToString());
		if (IsFolderAsset)
		{
			root.Add(FolderAssetName, true);
		}
		if (HasLicenseData)
		{
			root.Add(TimeCreatedName, CurrentTick);
			root.Add(LicenseTypeName, "Free");
		}
		//if (Importer.IncludesImporter(container.ExportVersion)) //For now, assume true
		{
			root.Add(Importer.ClassName, new ProjectYamlWalker(container) { ExportingAssetImporter = true }.ExportYamlNode(Importer));
		}
		return document;
	}

	public UnityGuid GUID { get; }
	public bool IsFolderAsset { get; }
	public bool HasLicenseData { get; }
	public IUnityObjectBase Importer { get; }

	private const long UnityEpoch = 0x089f7ff5f7b58000;

	private static long CurrentTick => (DateTime.Now.Ticks - UnityEpoch) / 10000000;

	public const string FileFormatVersionName = "fileFormatVersion";
	public const string GuidName = "guid";
	public const string FolderAssetName = "folderAsset";
	public const string TimeCreatedName = "timeCreated";
	public const string LicenseTypeName = "licenseType";
}
