using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.IO.Files;
using AssetRipper.VersionUtilities;
using AssetRipper.Yaml;


namespace AssetRipper.Assets.Metadata
{
	public sealed class Meta
	{
		public Meta(UnityGUID guid, IUnityObjectBase importer) : this(guid, importer, true) { }

		public Meta(UnityGUID guid, IUnityObjectBase importer, bool hasLicense) : this(guid, importer, hasLicense, false) { }

		public Meta(UnityGUID guid, IUnityObjectBase importer, bool hasLicense, bool isFolder)
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

		public static int ToFileFormatVersion(UnityVersion version)
		{
			//This has been 2 for a long time, but probably not forever.
			//If Unity 3 usesd version 1, we need to find out when 2 started.
			return 2;
		}

		public YamlDocument ExportYamlDocument(IExportContainer container)
		{
			YamlDocument document = new();
			YamlMappingNode root = document.CreateMappingRoot();
			root.Add(FileFormatVersionName, ToFileFormatVersion(container.ExportVersion));
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
				root.Add(Importer.ClassName, Importer.ExportYaml(container));
			}
			return document;
		}

		public UnityGUID GUID { get; }
		public bool IsFolderAsset { get; }
		public bool HasLicenseData { get; }
		public IUnityObjectBase Importer { get; }

		private static long CurrentTick => (DateTime.Now.Ticks - 0x089f7ff5f7b58000) / 10000000;

		public const string FileFormatVersionName = "fileFormatVersion";
		public const string GuidName = "guid";
		public const string FolderAssetName = "folderAsset";
		public const string TimeCreatedName = "timeCreated";
		public const string LicenseTypeName = "licenseType";
	}
}
