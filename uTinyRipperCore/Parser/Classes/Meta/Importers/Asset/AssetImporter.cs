using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes.AssetImporters;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	// NOTE: unknown layout for all importers for versions < 2.5.0
	public abstract class AssetImporter : NamedObject
	{
		protected AssetImporter(AssetLayout layout) :
			base(layout)
		{
			if (IncludesIDToName)
			{
				if (HasInternalIDToNameTable(layout.Info.Version))
				{
					InternalIDToNameTable = new Dictionary<Tuple<ClassIDType, long>, string>();
				}
				else if (FileIDToRecycleNameRelevant(layout.Info.Version))
				{
					FileIDToRecycleName = new Dictionary<long, string>();
				}
			}
			if (HasExternalObjects(layout.Info.Version))
			{
				ExternalObjects = new Dictionary<SourceAssetIdentifier, PPtr<Object>>();
			}
			if (HasUsedFileIDs(layout.Info.Version))
			{
				UsedFileIDs = Array.Empty<long>();
			}
			UserData = string.Empty;
			AssetBundleName = string.Empty;
			AssetBundleVariant = string.Empty;
		}

		protected AssetImporter(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasInternalIDToNameTable(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// Less than 3.5.0 or 4.5.2 and greater
		/// </summary>
		public static bool FileIDToRecycleNameRelevant(Version version) => version.IsGreaterEqual(4, 5, 2) || version.IsLess(3, 5);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasPreview(Version version) => version.IsLess(3);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasHash(Version version) => version.IsLess(3, 5);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasExternalObjects(Version version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasUsedFileIDs(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasAssetBundleName(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasAssetBundleVariant(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasUserData(Version version) => version.IsGreaterEqual(4);

		/// <summary>
		/// 4.5.2 and greater
		/// </summary>
		private static bool IsFileIDToRecycleNameConditional(Version version) => version.IsGreaterEqual(4, 5, 2);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsAlignExternalObjects(Version version) => version.IsGreaterEqual(2019);

		public abstract bool IncludesImporter(Version version);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasInternalIDToNameTable(reader.Version))
			{
				if (IncludesIDToName)
				{
					InternalIDToNameTable = new Dictionary<Tuple<ClassIDType, long>, string>();
					InternalIDToNameTable.Read(reader, (t) => (ClassIDType)t);
				}
			}
			else if (FileIDToRecycleNameRelevant(reader.Version))
			{
				if (!IsFileIDToRecycleNameConditional(reader.Version) || IncludesIDToName)
				{
					if (reader.Layout.PPtr.IsLongID)
					{
						FileIDToRecycleName = new Dictionary<long, string>();
						FileIDToRecycleName.Read(reader);
					}
					else
					{
						Dictionary<int, string> fileIDToRecycleName = new Dictionary<int, string>();
						fileIDToRecycleName.Read(reader);
						FileIDToRecycleNameInt = fileIDToRecycleName;
					}
				}
			}
			if (HasPreview(reader.Version))
			{
				Preview.Read(reader);
			}
			if (HasHash(reader.Version))
			{
				OldHashIdentity.Read(reader);
				NewHashIdentity.Read(reader);
			}
			if (HasExternalObjects(reader.Version))
			{
				ExternalObjects = new Dictionary<SourceAssetIdentifier, PPtr<Object>>();
				ExternalObjects.Read(reader);
				if (IsAlignExternalObjects(reader.Version))
				{
					reader.AlignStream();
				}
			}
			if (HasUsedFileIDs(reader.Version))
			{
				UsedFileIDs = reader.ReadInt64Array();
				reader.AlignStream();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasInternalIDToNameTable(writer.Version))
			{
				if (IncludesIDToName)
				{
					InternalIDToNameTable.Write(writer, (t) => (int)t);
				}
			}
			else if (FileIDToRecycleNameRelevant(writer.Version))
			{
				if (!IsFileIDToRecycleNameConditional(writer.Version) || IncludesIDToName)
				{
					if (writer.Layout.PPtr.IsLongID)
					{
						FileIDToRecycleName.Write(writer);
					}
					else
					{
						FileIDToRecycleNameInt.Write(writer);
					}
				}
			}
			if (HasPreview(writer.Version))
			{
				Preview.Write(writer);
			}
			if (HasHash(writer.Version))
			{
				OldHashIdentity.Write(writer);
				NewHashIdentity.Write(writer);
			}
			if (HasExternalObjects(writer.Version))
			{
				ExternalObjects.Write(writer);
				if (IsAlignExternalObjects(writer.Version))
				{
					writer.AlignStream();
				}
			}
			if (HasUsedFileIDs(writer.Version))
			{
				UsedFileIDs.Write(writer);
				writer.AlignStream();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasPreview(context.Version))
			{
				yield return context.FetchDependency(Preview, PreviewName);
			}
			if (HasExternalObjects(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(ExternalObjects.Select(t => t.Value), ExternalObjectsName))
				{
					yield return asset;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			if (HasInternalIDToNameTable(container.ExportVersion))
			{
				if (IncludesIDToName)
				{
					node.Add(InternalIDToNameTableName, InternalIDToNameTable.ExportYAML((t) => (int)t));
				}
			}
			else if (FileIDToRecycleNameRelevant(container.ExportVersion))
			{
				if (!IsFileIDToRecycleNameConditional(container.ExportVersion) || IncludesIDToName)
				{
					node.Add(FileIDToRecycleNameName, FileIDToRecycleName.ExportYAML());
				}
			}
			if (HasExternalObjects(container.ExportVersion))
			{
				node.Add(ExternalObjectsName, ExternalObjects.ExportYAML(container));
			}
			if (HasUsedFileIDs(container.ExportVersion))
			{
				node.Add(UsedFileIDsName, UsedFileIDs.ExportYAML(false));
			}
			if (HasPreview(container.ExportVersion))
			{
				node.Add(PreviewName, Preview.ExportYAML(container));
			}
			if (HasHash(container.ExportVersion))
			{
				node.Add(OldHashIdentityName, OldHashIdentity.ExportYAML(container));
				node.Add(NewHashIdentityName, NewHashIdentity.ExportYAML(container));
			}
			return node;
		}

		protected void PostRead(AssetReader reader)
		{
			if (HasUserData(reader.Version))
			{
				reader.AlignStream();

				UserData = reader.ReadString();
			}
			if (HasAssetBundleName(reader.Version))
			{
				AssetBundleName = reader.ReadString();
			}
			if (HasAssetBundleVariant(reader.Version))
			{
				AssetBundleVariant = reader.ReadString();
			}
		}

		protected void PostWrite(AssetWriter writer)
		{
			if (HasUserData(writer.Version))
			{
				writer.AlignStream();

				writer.Write(UserData);
			}
			if (HasAssetBundleName(writer.Version))
			{
				writer.Write(AssetBundleName);
			}
			if (HasAssetBundleVariant(writer.Version))
			{
				writer.Write(AssetBundleVariant);
			}
		}

		protected void PostExportYAML(IExportContainer container, YAMLMappingNode root)
		{
			if (HasUserData(container.ExportVersion))
			{
				root.Add(UserDataName, UserData);
			}
			if (HasAssetBundleName(container.ExportVersion))
			{
				root.Add(AssetBundleNameName, AssetBundleName);
			}
			if (HasAssetBundleVariant(container.ExportVersion))
			{
				root.Add(AssetBundleVariantName, AssetBundleVariant);
			}
		}

		public Dictionary<Tuple<ClassIDType, long>, string> InternalIDToNameTable { get; set; }
		public Dictionary<long, string> FileIDToRecycleName { get; set; }
		public Dictionary<SourceAssetIdentifier, PPtr<Object>> ExternalObjects { get; set; }
		public long[] UsedFileIDs { get; set; }
		public string UserData { get; set; }
		public string AssetBundleName { get; set; }
		public string AssetBundleVariant { get; set; }

		protected abstract bool IncludesIDToName { get; }

		private Dictionary<int, string> FileIDToRecycleNameInt
		{
			set
			{
				FileIDToRecycleName = new Dictionary<long, string>(value.Count);
				foreach (KeyValuePair<int, string> kvp in value)
				{
					FileIDToRecycleName.Add(kvp.Key, kvp.Value);
				}
			}
			get
			{
				Dictionary<int, string> result = new Dictionary<int, string>(FileIDToRecycleName.Count);
				foreach (KeyValuePair<long, string> kvp in FileIDToRecycleName)
				{
					result.Add((int)kvp.Key, kvp.Value);
				}
				return result;
			}
		}

		public const string InternalIDToNameTableName = "m_InternalIDToNameTable";
		public const string FileIDToRecycleNameName = "m_FileIDToRecycleName";
		public const string ExternalObjectsName = "m_ExternalObjects";
		public const string UsedFileIDsName = "m_UsedFileIDs";
		public const string UserDataName = "m_UserData";
		public const string AssetBundleNameName = "m_AssetBundleName";
		public const string AssetBundleVariantName = "m_AssetBundleVariant";
		public const string PreviewName = "m_Preview";
		public const string OldHashIdentityName = "m_OldHashIdentity";
		public const string NewHashIdentityName = "m_NewHashIdentity";

		public PPtr<Texture2D> Preview;
		public MdFour OldHashIdentity;
		public MdFour NewHashIdentity;
	}
}
