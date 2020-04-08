using System;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

using DateTime = System.DateTime;

namespace uTinyRipper.Classes
{
	public struct Meta
	{
		public Meta(UnityGUID guid, AssetImporter importer):
			this(guid, true, importer)
		{
		}

		public Meta(UnityGUID guid, bool hasLicense, AssetImporter importer):
			this(guid, hasLicense, false, importer)
		{
		}

		public Meta(UnityGUID guid, bool hasLicense, bool isFolder, AssetImporter importer)
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

		public static int ToFileFormatVersion(Version version)
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
			if (Importer.IncludesImporter(container.ExportVersion))
			{
				root.Add(Importer.ClassID.ToString(), Importer.ExportYAML(container));
			}
			return document;
		}

		public UnityGUID GUID { get; }
		public bool IsFolderAsset { get; }
		public bool HasLicenseData { get; }
		public AssetImporter Importer { get; }

		private long CurrentTick => (DateTime.Now.Ticks - 0x089f7ff5f7b58000) / 10000000;

		public const string FileFormatVersionName = "fileFormatVersion";
		public const string GuidName = "guid";
		public const string FolderAssetName = "folderAsset";
		public const string TimeCreatedName = "timeCreated";
		public const string LicenseTypeName = "licenseType";
	}
}
