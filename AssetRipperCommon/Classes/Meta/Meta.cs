using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using DateTime = System.DateTime;


namespace AssetRipper.Core.Classes.Meta
{
	public class Meta
	{
		public Meta(UnityGUID guid, IAssetImporter importer) : this(guid, true, importer) { }

		public Meta(UnityGUID guid, bool hasLicense, IAssetImporter importer) : this(guid, hasLicense, false, importer) { }

		public Meta(UnityGUID guid, bool hasLicense, bool isFolder, IAssetImporter importer)
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
#warning TODO:
			return 2;
		}

		public YAMLDocument ExportYAMLDocument(IExportContainer container)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Add(FileFormatVersionName, ToFileFormatVersion(container.ExportVersion));
			root.Add(GuidName, GUID.ExportYAML(container));
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
				root.Add(Importer.ClassID.ToString(), Importer.ExportYAML(container));
			}
			return document;
		}

		public UnityGUID GUID { get; }
		public bool IsFolderAsset { get; }
		public bool HasLicenseData { get; }
		public IAssetImporter Importer { get; }

		private long CurrentTick => (DateTime.Now.Ticks - 0x089f7ff5f7b58000) / 10000000;

		public const string FileFormatVersionName = "fileFormatVersion";
		public const string GuidName = "guid";
		public const string FolderAssetName = "folderAsset";
		public const string TimeCreatedName = "timeCreated";
		public const string LicenseTypeName = "licenseType";
	}
}
