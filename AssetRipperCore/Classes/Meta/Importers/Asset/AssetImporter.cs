﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AssetRipper.Core.Classes.Meta.Importers.Asset
{
	// NOTE: unknown layout for all importers for versions < 2.5.0
	public abstract class AssetImporter : NamedObject, IAssetImporter
	{
		protected AssetImporter(LayoutInfo layout) : base(layout)
		{
			if (IncludesIDToName)
			{
				if (HasInternalIDToNameTable(layout.Version))
				{
					InternalIDToNameTable = new Dictionary<Tuple<ClassIDType, long>, string>();
				}
				else if (FileIDToRecycleNameRelevant(layout.Version))
				{
					FileIDToRecycleName = new Dictionary<long, string>();
				}
			}
			if (HasExternalObjects(layout.Version))
			{
				ExternalObjects = new Dictionary<SourceAssetIdentifier, PPtr<Object.Object>>();
			}
			if (HasUsedFileIDs(layout.Version))
			{
				UsedFileIDs = Array.Empty<long>();
			}
			UserData = string.Empty;
			AssetBundleName = string.Empty;
			AssetBundleVariant = string.Empty;
		}

		protected AssetImporter(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasInternalIDToNameTable(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// Less than 3.5.0 or 4.5.2 and greater
		/// </summary>
		public static bool FileIDToRecycleNameRelevant(UnityVersion version) => version.IsGreaterEqual(4, 5, 2) || version.IsLess(3, 5);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasPreview(UnityVersion version) => version.IsLess(3);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasHash(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasExternalObjects(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasUsedFileIDs(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasAssetBundleName(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasAssetBundleVariant(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasUserData(UnityVersion version) => version.IsGreaterEqual(4);

		/// <summary>
		/// 4.5.2 and greater
		/// </summary>
		private static bool IsFileIDToRecycleNameConditional(UnityVersion version) => version.IsGreaterEqual(4, 5, 2);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsAlignExternalObjects(UnityVersion version) => version.IsGreaterEqual(2019);

		public abstract bool IncludesImporter(UnityVersion version);

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
					if (PPtr.IsLongID(reader.Version))
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
				ExternalObjects = new Dictionary<SourceAssetIdentifier, PPtr<Object.Object>>();
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
					if (PPtr.IsLongID(writer.Version))
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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasPreview(context.Version))
			{
				yield return context.FetchDependency(Preview, PreviewName);
			}
			if (HasExternalObjects(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(ExternalObjects.Select(t => t.Value), ExternalObjectsName))
				{
					yield return asset;
				}
			}
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			if (HasInternalIDToNameTable(container.ExportVersion))
			{
				if (IncludesIDToName)
				{
					node.Add(InternalIDToNameTableName, InternalIDToNameTable.ExportYaml((t) => (int)t));
				}
			}
			else if (FileIDToRecycleNameRelevant(container.ExportVersion))
			{
				if (!IsFileIDToRecycleNameConditional(container.ExportVersion) || IncludesIDToName)
				{
					node.Add(FileIDToRecycleNameName, FileIDToRecycleName.ExportYaml());
				}
			}
			if (HasExternalObjects(container.ExportVersion))
			{
				node.Add(ExternalObjectsName, ExternalObjects.ExportYaml(container));
			}
			if (HasUsedFileIDs(container.ExportVersion))
			{
				node.Add(UsedFileIDsName, UsedFileIDs.ExportYaml(false));
			}
			if (HasPreview(container.ExportVersion))
			{
				node.Add(PreviewName, Preview.ExportYaml(container));
			}
			if (HasHash(container.ExportVersion))
			{
				node.Add(OldHashIdentityName, OldHashIdentity.ExportYaml(container));
				node.Add(NewHashIdentityName, NewHashIdentity.ExportYaml(container));
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

		protected void PostExportYaml(IExportContainer container, YamlMappingNode root)
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
		public Dictionary<SourceAssetIdentifier, PPtr<Object.Object>> ExternalObjects { get; set; }
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

		public PPtr<Texture2D.Texture2D> Preview = new();
		public MdFour OldHashIdentity = new();
		public MdFour NewHashIdentity = new();
	}
}
